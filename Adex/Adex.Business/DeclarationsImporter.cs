using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adex.Data.Model;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Adex.Business
{
    /// <summary>
    /// Replaces the whole content of the normalized database with the consolidated
    /// declarations.csv export: the file is streamed to PostgreSQL with COPY into a staging
    /// table, then dispatched with set-based SQL. Everything runs in a single transaction, so a
    /// failed or cancelled import leaves the previous data untouched.
    /// </summary>
    public sealed class DeclarationsImporter
    {
        private static readonly string[] ExpectedColumns =
        {
            "id", "token", "entreprise_id", "lien_interet", "identifiant_unique",
            "convention_liee", "motif_lien_interet_code", "motif_lien_interet", "autre_motif",
            "information_evenement", "montant", "date", "date_debut", "date_fin",
            "id_beneficiaire", "identite", "prenom", "beneficiaire_categorie_code",
            "beneficiaire_categorie", "beneficiaire_type_code", "beneficiaire_type",
            "beneficiaire_identifiant", "beneficiaire_profession_code", "profession_libelle",
            "structure_exercice", "pays_code", "adresse", "code_postal", "ville",
            "demande_de_rectification", "statut", "date_publication", "date_transmission",
            "raison_sociale", "numero_siren", "secteur_activite", "mere_id",
            "Ville de l'entreprise", "Département de l'entreprise", "Région de l'entreprise",
            "entreprise_nom_pays", "code_postal_entreprise", "ville_sanscedex", "dep_code",
            "reg_code", "reg_name", "dep_name", "beneficiaire_nom_prenom_com_name",
            "semestre_annee", "pays_libelle", "Nom Pays (Bénéficiaire)",
        };

        private const string CreateStageSql = """
            CREATE TEMP TABLE stage (
                id text, token text, entreprise_id text, lien_interet text, identifiant_unique text,
                convention_liee text, motif_code text, motif text, autre_motif text,
                information_evenement text, montant text, date text, date_debut text, date_fin text,
                id_beneficiaire text, identite text, prenom text, categorie_code text,
                categorie text, type_code text, type text, beneficiaire_identifiant text,
                profession_code text, profession text, structure_exercice text, pays_code text,
                adresse text, code_postal text, ville text, demande_rectification text,
                statut text, date_publication text, date_transmission text, raison_sociale text,
                siren text, secteur_activite text, mere_id text, ville_entreprise text,
                departement_entreprise text, region_entreprise text, pays_entreprise text,
                code_postal_entreprise text, ville_sanscedex text, dep_code text, reg_code text,
                reg_name text, dep_name text, nom_prenom_com text, semestre text,
                pays_libelle text, pays_beneficiaire text
            ) ON COMMIT DROP
            """;

        private const string CopySql =
            "COPY stage FROM STDIN (FORMAT csv, DELIMITER ';', QUOTE '\"', HEADER true, ENCODING 'UTF8')";

        private static readonly string[] TransformSteps =
        {
            "Entreprises",
            "Bénéficiaires",
            "Liens",
            "Montants",
            "Attributs des liens",
        };

        private const string TruncateSql = """
            TRUNCATE TABLE "EntityTotals", "EntityAttributes", "FinancialLinks", "Links", "Persons", "Companies", "Entities"
            RESTART IDENTITY
            """;

        private const string SafeTimestampFunctionSql = """
            CREATE FUNCTION pg_temp.safe_ts(value text) RETURNS timestamp
            LANGUAGE plpgsql IMMUTABLE AS $$
            BEGIN
                RETURN CASE WHEN value ~ '^\d{4}-\d{2}-\d{2}' THEN left(replace(value, 'T', ' '), 19)::timestamp END;
            EXCEPTION WHEN others THEN
                RETURN NULL;
            END
            $$
            """;

        private const string ValidRows = """
            nullif(trim(entreprise_id), '') IS NOT NULL
            AND nullif(trim(id_beneficiaire), '') IS NOT NULL
            AND nullif(trim(id), '') IS NOT NULL
            """;

        // The sort only carries (key, date, ctid); wide rows are fetched afterwards by ctid,
        // which keeps temporary disk usage small.
        private static readonly string[] CompanySourceSql =
        {
            $"""
            CREATE TEMP TABLE company_pick ON COMMIT DROP AS
            SELECT DISTINCT ON (k) rid
            FROM (
                SELECT ctid AS rid, trim(entreprise_id) AS k, date_publication AS d
                FROM stage WHERE {ValidRows}
            ) x
            ORDER BY k, d DESC NULLS LAST
            """,
            """
            CREATE TEMP TABLE company_src ON COMMIT DROP AS
            SELECT gen_random_uuid() AS id, trim(s.entreprise_id) AS k, s.raison_sociale, s.siren,
                s.secteur_activite, s.mere_id, s.ville_entreprise, s.departement_entreprise,
                s.region_entreprise, s.pays_entreprise, s.code_postal_entreprise
            FROM company_pick p JOIN stage s ON s.ctid = p.rid
            """,
            "DROP TABLE company_pick",
        };

        private static readonly string[] PersonSourceSql =
        {
            $"""
            CREATE TEMP TABLE person_pick ON COMMIT DROP AS
            SELECT DISTINCT ON (k) rid
            FROM (
                SELECT ctid AS rid, trim(id_beneficiaire) AS k, date_publication AS d
                FROM stage WHERE {ValidRows}
            ) x
            ORDER BY k, d DESC NULLS LAST
            """,
            """
            CREATE TEMP TABLE person_src ON COMMIT DROP AS
            SELECT gen_random_uuid() AS id, trim(s.id_beneficiaire) AS k, s.identite, s.prenom, s.categorie,
                s.type, s.beneficiaire_identifiant, s.profession, s.structure_exercice,
                s.adresse, s.code_postal, s.ville,
                coalesce(nullif(trim(s.pays_libelle), ''), s.pays_beneficiaire) AS pays
            FROM person_pick p JOIN stage s ON s.ctid = p.rid
            """,
            "DROP TABLE person_pick",
        };

        private static readonly string[] CompanyInsertSql =
        {
            """INSERT INTO "Entities"("Id") SELECT id FROM company_src""",
            """
            INSERT INTO "Companies"("Id", "Designation")
            SELECT s.id, left(nullif(trim(s.raison_sociale), ''), 200)
            FROM company_src s
            """,
            """
            INSERT INTO "EntityAttributes"("EntityId", "Name", "Value")
            SELECT s.id, a.name, trim(a.value)
            FROM company_src s
            CROSS JOIN LATERAL (VALUES
                ('Référence', s.k),
                ('SIREN', s.siren),
                ('Secteur d''activité', s.secteur_activite),
                ('Société mère', s.mere_id),
                ('Ville', s.ville_entreprise),
                ('Code postal', s.code_postal_entreprise),
                ('Département', s.departement_entreprise),
                ('Région', s.region_entreprise),
                ('Pays', s.pays_entreprise)
            ) AS a(name, value)
            WHERE nullif(trim(a.value), '') IS NOT NULL
            ON CONFLICT DO NOTHING
            """,
        };

        private static readonly string[] PersonInsertSql =
        {
            """INSERT INTO "Entities"("Id") SELECT id FROM person_src""",
            """
            INSERT INTO "Persons"("Id", "LastName", "FirstName")
            SELECT s.id, left(nullif(trim(s.identite), ''), 200), left(nullif(trim(s.prenom), ''), 200)
            FROM person_src s
            """,
            """
            INSERT INTO "EntityAttributes"("EntityId", "Name", "Value")
            SELECT s.id, a.name, trim(a.value)
            FROM person_src s
            CROSS JOIN LATERAL (VALUES
                ('Référence', s.k),
                ('Catégorie', s.categorie),
                ('Type d''identifiant', s.type),
                ('Identifiant', s.beneficiaire_identifiant),
                ('Profession', s.profession),
                ('Structure d''exercice', s.structure_exercice),
                ('Adresse', replace(s.adresse, '==', ', ')),
                ('Code postal', s.code_postal),
                ('Ville', s.ville),
                ('Pays', s.pays)
            ) AS a(name, value)
            WHERE nullif(trim(a.value), '') IS NOT NULL
            ON CONFLICT DO NOTHING
            """,
        };

        private static readonly string[] LinkSourceSql =
        {
            $"""
            CREATE TEMP TABLE link_pick ON COMMIT DROP AS
            SELECT DISTINCT ON (k) rid
            FROM (
                SELECT ctid AS rid, trim(id) AS k, date_publication AS d
                FROM stage WHERE {ValidRows}
            ) x
            ORDER BY k, d DESC NULLS LAST
            """,
            """
            CREATE TEMP TABLE link_src ON COMMIT DROP AS
            SELECT gen_random_uuid() AS id, rid FROM link_pick
            """,
            "DROP TABLE link_pick",
        };

        private static readonly string[] LinkInsertSql =
        {
            """INSERT INTO "Entities"("Id") SELECT id FROM link_src""",
            """
            INSERT INTO "Links"("Id", "From_Id", "To_Id", "Kind", "Date")
            SELECT l.id, c.id, b.id, nullif(trim(s.lien_interet), ''),
                coalesce(pg_temp.safe_ts(s.date), pg_temp.safe_ts(s.date_debut),
                    pg_temp.safe_ts(s.date_publication), timestamp '1900-01-01')
            FROM link_src l
            JOIN stage s ON s.ctid = l.rid
            JOIN company_src c ON c.k = trim(s.entreprise_id)
            JOIN person_src b ON b.k = trim(s.id_beneficiaire)
            """,
        };

        private const string FinancialLinkInsertSql = """
            INSERT INTO "FinancialLinks"("Id", "Amount", "DeclarationType")
            SELECT l.id,
                CASE WHEN replace(trim(s.montant), ',', '.') ~ '^-?\d+(\.\d+)?$'
                    THEN replace(trim(s.montant), ',', '.')::numeric ELSE 0 END,
                CASE lower(trim(s.lien_interet))
                    WHEN 'avantage' THEN 'Avantage'
                    WHEN 'convention' THEN 'Convention'
                    WHEN 'remuneration' THEN 'Rémunération'
                    WHEN 'rémunération' THEN 'Rémunération'
                    ELSE 'Non renseigné' END
            FROM link_src l
            JOIN stage s ON s.ctid = l.rid
            """;

        // Same definitions as the EF migration; they are dropped before the bulk load and
        // rebuilt afterwards, which is much faster than maintaining them row by row.
        private static readonly (string Name, string Sql)[] SecondaryIndexes =
        {
            ("IX_Links_From_Id_Date", """CREATE INDEX "IX_Links_From_Id_Date" ON "Links" ("From_Id", "Date")"""),
            ("IX_Links_To_Id_Date", """CREATE INDEX "IX_Links_To_Id_Date" ON "Links" ("To_Id", "Date")"""),
            ("IX_Companies_Designation", """CREATE INDEX "IX_Companies_Designation" ON "Companies" USING gin ("Designation" gin_trgm_ops)"""),
            ("IX_Persons_LastName", """CREATE INDEX "IX_Persons_LastName" ON "Persons" USING gin ("LastName" gin_trgm_ops)"""),
            ("IX_Persons_FirstName", """CREATE INDEX "IX_Persons_FirstName" ON "Persons" USING gin ("FirstName" gin_trgm_ops)"""),
            ("IX_EntityTotals_Total", """CREATE INDEX "IX_EntityTotals_Total" ON "EntityTotals" ("Total")"""),
        };

        private const string EntityTotalsInsertSql = """
            INSERT INTO "EntityTotals"("EntityId", "LinkCount", "OutgoingAmount", "IncomingAmount", "Total")
            SELECT id, sum(cnt), sum(outgoing), sum(incoming), sum(outgoing + incoming)
            FROM (
                SELECT l."From_Id" AS id, count(*) AS cnt, sum(f."Amount") AS outgoing, 0::numeric AS incoming
                FROM "Links" l JOIN "FinancialLinks" f ON f."Id" = l."Id"
                GROUP BY l."From_Id"
                UNION ALL
                SELECT l."To_Id", count(*), 0::numeric, sum(f."Amount")
                FROM "Links" l JOIN "FinancialLinks" f ON f."Id" = l."Id"
                GROUP BY l."To_Id"
            ) x
            GROUP BY id
            """;

        private const string LinkAttributeInsertSql = """
            INSERT INTO "EntityAttributes"("EntityId", "Name", "Value")
            SELECT l.id, a.name, trim(a.value)
            FROM link_src l
            JOIN stage s ON s.ctid = l.rid
            CROSS JOIN LATERAL (VALUES
                ('Référence', s.id),
                ('Statut', s.statut),
                ('Motif', s.motif),
                ('Autre motif', s.autre_motif),
                ('Information sur l''événement', s.information_evenement),
                ('Date de début', left(s.date_debut, 10)),
                ('Date de fin', left(s.date_fin, 10)),
                ('Date de publication', left(s.date_publication, 10)),
                ('Convention liée', s.convention_liee),
                ('Identifiant unique', s.identifiant_unique),
                ('Semestre', s.semestre)
            ) AS a(name, value)
            WHERE nullif(trim(a.value), '') IS NOT NULL
            ON CONFLICT DO NOTHING
            """;

        private readonly IDbContextFactory<AdexContext> _contextFactory;
        private readonly ILogger<DeclarationsImporter> _logger;
        private readonly Stopwatch _total = new();

        public DeclarationsImporter(
            IDbContextFactory<AdexContext> contextFactory,
            ILogger<DeclarationsImporter> logger
        )
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        public async Task<DeclarationsImportResult> ImportAsync(
            string path,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            var connectionString = await GetConnectionStringAsync();
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

            _total.Restart();
            _logger.LogInformation("Import: démarrage ({Path})", path);

            progress?.Report("Réinitialisation des tables");
            await ExecuteAsync(connection, "Réinitialisation des tables", TruncateSql, progress, cancellationToken);
            foreach (var index in SecondaryIndexes)
            {
                await ExecuteAsync(
                    connection,
                    $"Suppression de l'index {index.Name}",
                    $"DROP INDEX IF EXISTS \"{index.Name}\"",
                    progress,
                    cancellationToken
                );
            }
            await ExecuteAsync(connection, "Création de la table de transit", CreateStageSql, progress, cancellationToken);
            await ExecuteAsync(connection, "Création de la fonction de dates", SafeTimestampFunctionSql, progress, cancellationToken);

            await CopyFileAsync(connection, path, progress, cancellationToken);

            var staged = await ScalarAsync(connection, "Comptage des lignes chargées", "SELECT count(*) FROM stage", cancellationToken);
            var valid = await ScalarAsync(
                connection,
                "Comptage des lignes valides",
                $"SELECT count(*) FROM stage WHERE {ValidRows}",
                cancellationToken
            );
            _logger.LogInformation(
                "Import: {Staged} lignes chargées, {Valid} valides, {Skipped} ignorées",
                staged,
                valid,
                staged - valid
            );

            progress?.Report(TransformSteps[0]);
            await ExecuteAllAsync(connection, TransformSteps[0] + " (sélection)", CompanySourceSql, progress, cancellationToken);
            await ExecuteAllAsync(connection, TransformSteps[0] + " (insertion)", CompanyInsertSql, progress, cancellationToken);

            progress?.Report(TransformSteps[1]);
            await ExecuteAllAsync(connection, TransformSteps[1] + " (sélection)", PersonSourceSql, progress, cancellationToken);
            await ExecuteAllAsync(connection, TransformSteps[1] + " (insertion)", PersonInsertSql, progress, cancellationToken);

            progress?.Report(TransformSteps[2]);
            await ExecuteAllAsync(connection, TransformSteps[2] + " (sélection)", LinkSourceSql, progress, cancellationToken);
            await ExecuteAllAsync(connection, TransformSteps[2], LinkInsertSql, progress, cancellationToken);

            progress?.Report(TransformSteps[3]);
            await ExecuteAsync(connection, TransformSteps[3], FinancialLinkInsertSql, progress, cancellationToken);

            progress?.Report(TransformSteps[4]);
            await ExecuteAsync(connection, TransformSteps[4], LinkAttributeInsertSql, progress, cancellationToken);

            await ExecuteAsync(connection, "Totaux par entité", EntityTotalsInsertSql, progress, cancellationToken);
            await ExecuteAsync(connection, "Paramétrage mémoire", "SET LOCAL maintenance_work_mem = '512MB'", null, cancellationToken);
            foreach (var index in SecondaryIndexes)
            {
                await ExecuteAsync(connection, $"Création de l'index {index.Name}", index.Sql, progress, cancellationToken);
            }
            var result = new DeclarationsImportResult
            {
                Rows = staged,
                SkippedRows = staged - valid,
                Companies = await ScalarAsync(connection, "Nombre d'entreprises", "SELECT count(*) FROM \"Companies\"", cancellationToken),
                Beneficiaries = await ScalarAsync(connection, "Nombre de bénéficiaires", "SELECT count(*) FROM \"Persons\"", cancellationToken),
                Links = await ScalarAsync(connection, "Nombre de liens", "SELECT count(*) FROM \"FinancialLinks\"", cancellationToken),
            };

            progress?.Report("Validation de la transaction");
            _logger.LogInformation("Import: validation de la transaction (COMMIT)");
            await transaction.CommitAsync(cancellationToken);
            await ExecuteAsync(
                connection,
                "Statistiques (VACUUM ANALYZE)",
                """VACUUM (ANALYZE) "Entities", "Companies", "Persons", "Links", "FinancialLinks", "EntityAttributes", "EntityTotals" """,
                progress,
                cancellationToken
            );
            _logger.LogInformation(
                "Import terminé en {Elapsed}: {Companies} entreprises, {Persons} bénéficiaires, {Links} liens",
                _total.Elapsed,
                result.Companies,
                result.Beneficiaries,
                result.Links
            );
            return result;
        }

        private async Task<string> GetConnectionStringAsync()
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return db.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The 'Adex' connection string is not configured.");
        }

        private async Task CopyFileAsync(
            NpgsqlConnection connection,
            string path,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            await using var file = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1 << 20,
                FileOptions.SequentialScan | FileOptions.Asynchronous
            );
            using var reader = new StreamReader(file, new UTF8Encoding(false), true, 1 << 20);
            var header = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;
            var actual = header.TrimStart('\uFEFF').Split(';');
            if (!actual.SequenceEqual(ExpectedColumns, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Unexpected declarations.csv header: expected {ExpectedColumns.Length} known columns, found {actual.Length}."
                );
            }

            file.Position = 0;
            reader.DiscardBufferedData();

            _logger.LogInformation("Import: chargement du fichier ({Size:N0} Mo)", file.Length >> 20);
            var copyWatch = Stopwatch.StartNew();
            await using (var writer = await connection.BeginTextImportAsync(CopySql, cancellationToken))
            {
                var buffer = new char[1 << 16];
                var lastReport = DateTime.UtcNow;
                int read;
                while ((read = await reader.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
                {
                    await writer.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                    if (DateTime.UtcNow - lastReport > TimeSpan.FromSeconds(10))
                    {
                        lastReport = DateTime.UtcNow;
                        var done = file.Position;
                        var percent = done * 100 / file.Length;
                        var perSecond = done / Math.Max(1, copyWatch.Elapsed.TotalSeconds);
                        var remaining = TimeSpan.FromSeconds((file.Length - done) / Math.Max(1, perSecond));
                        var message =
                            $"Chargement du fichier : {percent} % ({done >> 20:N0}/{file.Length >> 20:N0} Mo, {perSecond / (1 << 20):N1} Mo/s, reste ~{remaining:hh\\:mm\\:ss})";
                        progress?.Report(message);
                        _logger.LogInformation("Import: {Message}", message);
                    }
                }
            }

            _logger.LogInformation("Import: fichier chargé en {Elapsed} (total {Total})", copyWatch.Elapsed, _total.Elapsed);
        }

        private async Task ExecuteAllAsync(
            NpgsqlConnection connection,
            string label,
            string[] statements,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            for (var i = 0; i < statements.Length; i++)
            {
                await ExecuteAsync(
                    connection,
                    $"{label} [{i + 1}/{statements.Length}]",
                    statements[i],
                    progress,
                    cancellationToken
                );
            }
        }

        private async Task ExecuteAsync(
            NpgsqlConnection connection,
            string label,
            string sql,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            progress?.Report(label);
            _logger.LogInformation("Import: {Label}…", label);
            var watch = Stopwatch.StartNew();
            await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 0 };
            var rows = await command.ExecuteNonQueryAsync(cancellationToken);
            _logger.LogInformation(
                "Import: {Label} terminé en {Elapsed} ({Rows} lignes, total {Total})",
                label,
                watch.Elapsed,
                rows,
                _total.Elapsed
            );
        }

        private async Task<long> ScalarAsync(
            NpgsqlConnection connection,
            string label,
            string sql,
            CancellationToken cancellationToken
        )
        {
            var watch = Stopwatch.StartNew();
            await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 0 };
            var value = Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
            _logger.LogInformation("Import: {Label} = {Value} ({Elapsed})", label, value, watch.Elapsed);
            return value;
        }
    }

    public sealed class DeclarationsImportResult
    {
        public long Rows { get; set; }

        public long SkippedRows { get; set; }

        public long Companies { get; set; }

        public long Beneficiaries { get; set; }

        public long Links { get; set; }
    }
}

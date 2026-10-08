using Adex.Data.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

using NpgsqlTypes;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.Business
{
    /// <summary>
    /// Replaces the whole content of the normalized database with the consolidated
    /// declarations.csv export. The file is read twice and streamed straight into the final
    /// tables with binary COPY (no staging table, no transaction): pass 1 picks, for every
    /// company, beneficiary and link, the most recently published row; pass 2 writes the
    /// picked rows. A failed import is simply restarted from the beginning.
    /// </summary>
    public sealed class DeclarationsImporter
    {
        private static class Col
        {
            public const int Id = 0;
            public const int CompanyId = 2;
            public const int Kind = 3;
            public const int UniqueId = 4;
            public const int LinkedConvention = 5;
            public const int Reason = 7;
            public const int OtherReason = 8;
            public const int EventInfo = 9;
            public const int Amount = 10;
            public const int Date = 11;
            public const int StartDate = 12;
            public const int EndDate = 13;
            public const int PersonId = 14;
            public const int LastName = 15;
            public const int FirstName = 16;
            public const int Category = 18;
            public const int PersonType = 20;
            public const int PersonIdentifier = 21;
            public const int Profession = 23;
            public const int Structure = 24;
            public const int Address = 26;
            public const int PersonZip = 27;
            public const int PersonCity = 28;
            public const int Status = 30;
            public const int PublicationDate = 31;
            public const int CompanyName = 33;
            public const int Siren = 34;
            public const int Sector = 35;
            public const int Parent = 36;
            public const int CompanyCity = 37;
            public const int CompanyDepartment = 38;
            public const int CompanyRegion = 39;
            public const int CompanyCountry = 40;
            public const int CompanyZip = 41;
            public const int Semester = 48;
            public const int CountryLabel = 49;
            public const int PersonCountry = 50;
        }

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

        private const string TruncateSql = """
            TRUNCATE TABLE "FinancialLinkTypeTotals", "EntityTotals", "EntityAttributes", "FinancialLinks", "Links", "Persons", "Companies", "Entities"
            RESTART IDENTITY
            """;

        // Integrity is guaranteed by construction (every id written comes from the same
        // dictionaries), so the foreign keys are dropped for the load and recreated NOT VALID.
        private static readonly (string Table, string Name, string Column, string Principal)[] ForeignKeys =
        {
            ("Companies", "FK_Companies_Entities_Id", "Id", "Entities"),
            ("Persons", "FK_Persons_Entities_Id", "Id", "Entities"),
            ("EntityAttributes", "FK_EntityAttributes_Entities_EntityId", "EntityId", "Entities"),
            ("Links", "FK_Links_Entities_Id", "Id", "Entities"),
            ("Links", "FK_Links_Entities_From_Id", "From_Id", "Entities"),
            ("Links", "FK_Links_Entities_To_Id", "To_Id", "Entities"),
            ("FinancialLinks", "FK_FinancialLinks_Links_Id", "Id", "Links"),
        };

        // Same definitions as the EF migration; dropped before the load and rebuilt afterwards.
        // Uniqueness of the EntityAttributes key is guaranteed by the importer (one row per entity).
        private static readonly (string Name, string Sql, string Drop)[] Indexes =
        {
            ("PK_EntityAttributes", """ALTER TABLE "EntityAttributes" ADD CONSTRAINT "PK_EntityAttributes" PRIMARY KEY ("EntityId")""", """ALTER TABLE "EntityAttributes" DROP CONSTRAINT IF EXISTS "PK_EntityAttributes" """),
            ("IX_Links_From_Id_Date", """CREATE INDEX "IX_Links_From_Id_Date" ON "Links" ("From_Id", "Date")""", null),
            ("IX_Links_To_Id_Date", """CREATE INDEX "IX_Links_To_Id_Date" ON "Links" ("To_Id", "Date")""", null),
            ("IX_Companies_Designation", """CREATE INDEX "IX_Companies_Designation" ON "Companies" USING gin ("Designation" gin_trgm_ops)""", null),
            ("IX_Persons_LastName", """CREATE INDEX "IX_Persons_LastName" ON "Persons" USING gin ("LastName" gin_trgm_ops)""", null),
            ("IX_Persons_FirstName", """CREATE INDEX "IX_Persons_FirstName" ON "Persons" USING gin ("FirstName" gin_trgm_ops)""", null),
            ("IX_EntityTotals_Total", """CREATE INDEX "IX_EntityTotals_Total" ON "EntityTotals" ("Total")""", null),
        };

        private static readonly string[] Tables =
        {
            "Entities", "Companies", "Persons", "Links", "FinancialLinks", "EntityAttributes", "EntityTotals", "FinancialLinkTypeTotals",
        };

        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy-MM-dd",
        };

        private const int MaxParallelMaintenance = 3;
        private const int LinkAttributeWriters = 2;
        private static readonly TimeSpan ReportInterval = TimeSpan.FromSeconds(10);

        private struct Winner
        {
            public long Row;
            public long Date;
            public Guid Id;
        }

        private struct LinkWinner
        {
            public long Row;
            public long Date;
        }

        private struct Totals
        {
            public int Count;
            public decimal Outgoing;
            public decimal Incoming;
        }

        private readonly record struct CompanyRow(Guid Id, string Designation);

        private readonly record struct PersonRow(Guid Id, string LastName, string FirstName);

        private readonly record struct LinkRow(Guid Id, Guid From, Guid To, string Kind, DateTime Date);

        private readonly record struct FinancialRow(Guid Id, decimal Amount, string Type);

        private readonly record struct AttributeRow(Guid EntityId, string Json);

        // Gathers the attributes of one entity (consecutive calls with the same id) into a single jsonb row.
        private sealed class AttributeSink
        {
            private readonly ImportTableWriter<AttributeRow> _writer;
            private readonly ArrayBufferWriter<byte> _buffer = new();
            private Utf8JsonWriter _json;
            private Guid _current;
            private bool _open;

            public AttributeSink(ImportTableWriter<AttributeRow> writer)
            {
                _writer = writer;
            }

            public void Add(Guid entityId, string name, string value)
            {
                if (_open && entityId != _current)
                {
                    Flush();
                }

                if (!_open)
                {
                    _buffer.Clear();
                    _json = new Utf8JsonWriter(_buffer);
                    _json.WriteStartObject();
                    _current = entityId;
                    _open = true;
                }

                _json.WriteString(name, value);
            }

            public void Flush()
            {
                if (!_open)
                {
                    return;
                }

                _json.WriteEndObject();
                _json.Flush();
                _writer.Add(new AttributeRow(_current, Encoding.UTF8.GetString(_buffer.WrittenSpan)));
                _json.Dispose();
                _open = false;
            }
        }

        private readonly record struct TotalRow(Guid Id, int Count, decimal Outgoing, decimal Incoming);

        private sealed class ScanResult
        {
            public Dictionary<long, Winner> Companies { get; } = new();

            public Dictionary<long, Winner> Persons { get; } = new();

            public Dictionary<long, LinkWinner> Links { get; } = new();

            public long Rows { get; set; }

            public long Skipped { get; set; }
        }

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
            _total.Restart();
            _logger.LogInformation("Import: démarrage ({Path})", path);

            progress?.Report("Analyse du fichier (passe 1/2)");
            var scan = await Task.Factory.StartNew(
                () => Scan(path, progress, cancellationToken),
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
            _logger.LogInformation(
                "Import: {Rows} lignes lues, {Skipped} ignorées, {Companies} entreprises, {Persons} bénéficiaires, {Links} liens (total {Total})",
                scan.Rows,
                scan.Skipped,
                scan.Companies.Count,
                scan.Persons.Count,
                scan.Links.Count,
                _total.Elapsed
            );

            await ExecuteAsync(connectionString, "Suppression des clés étrangères", ForeignKeys.Select(x => $"""ALTER TABLE "{x.Table}" DROP CONSTRAINT IF EXISTS "{x.Name}" """), progress, cancellationToken);
            await ExecuteAsync(connectionString, "Suppression des index", Indexes.Select(x => x.Drop ?? $"""DROP INDEX IF EXISTS "{x.Name}" """), progress, cancellationToken);
            await ExecuteAsync(connectionString, "Réinitialisation des tables", new[] { TruncateSql }, progress, cancellationToken);

            progress?.Report("Chargement des données (passe 2/2)");
            await LoadAsync(path, connectionString, scan, progress, cancellationToken);

            await RebuildStructureAsync(connectionString, progress, cancellationToken);

            var result = new DeclarationsImportResult
            {
                Rows = scan.Rows,
                SkippedRows = scan.Skipped,
                Companies = scan.Companies.Count,
                Beneficiaries = scan.Persons.Count,
                Links = scan.Links.Count,
            };
            _logger.LogInformation(
                "Import terminé en {Elapsed}: {Companies} entreprises, {Persons} bénéficiaires, {Links} liens",
                _total.Elapsed,
                result.Companies,
                result.Beneficiaries,
                result.Links
            );
            return result;
        }

        private ScanResult Scan(string path, IProgress<string> progress, CancellationToken cancellationToken)
        {
            using var file = OpenFile(path);
            using var reader = new CsvRecordReader(new StreamReader(file, new UTF8Encoding(false), true, 1 << 20));
            ReadHeader(reader);

            var keep = new bool[ExpectedColumns.Length];
            keep[Col.Id] = keep[Col.CompanyId] = keep[Col.PersonId] = keep[Col.PublicationDate] = true;
            var fields = new string[ExpectedColumns.Length];
            var scan = new ScanResult();
            var watch = Stopwatch.StartNew();
            var lastReport = watch.Elapsed;
            long row = 0;

            while (reader.ReadRecord(fields, keep))
            {
                var currentRow = row++;
                if ((currentRow & 0xFFF) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (watch.Elapsed - lastReport > ReportInterval)
                    {
                        lastReport = watch.Elapsed;
                        Report(progress, "Analyse du fichier", file, watch.Elapsed, currentRow);
                    }
                }

                var companyKey = fields[Col.CompanyId].AsSpan().Trim(' ');
                var personKey = fields[Col.PersonId].AsSpan().Trim(' ');
                var linkKey = fields[Col.Id].AsSpan().Trim(' ');
                if (companyKey.IsEmpty || personKey.IsEmpty || linkKey.IsEmpty)
                {
                    scan.Skipped++;
                    continue;
                }

                var date = DateKey(fields[Col.PublicationDate]);
                Pick(scan.Companies, Hash(companyKey), currentRow, date);
                Pick(scan.Persons, Hash(personKey), currentRow, date);

                ref var link = ref CollectionsMarshal.GetValueRefOrAddDefault(scan.Links, Hash(linkKey), out var exists);
                if (!exists || date > link.Date)
                {
                    link.Row = currentRow;
                    link.Date = date;
                }
            }

            scan.Rows = row;
            return scan;
        }

        private static void Pick(Dictionary<long, Winner> winners, long hash, long row, long date)
        {
            ref var winner = ref CollectionsMarshal.GetValueRefOrAddDefault(winners, hash, out var exists);
            if (!exists)
            {
                winner.Row = row;
                winner.Date = date;
                winner.Id = Guid.CreateVersion7();
            }
            else if (date > winner.Date)
            {
                winner.Row = row;
                winner.Date = date;
            }
        }

        private async Task LoadAsync(
            string path,
            string connectionString,
            ScanResult scan,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            using var failure = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var entities = new ImportTableWriter<Guid>(
                connectionString,
                """COPY "Entities"("Id") FROM STDIN (FORMAT BINARY)""",
                (w, v) => w.Write(v, NpgsqlDbType.Uuid),
                failure
            );
            var companies = new ImportTableWriter<CompanyRow>(
                connectionString,
                """COPY "Companies"("Id", "Designation") FROM STDIN (FORMAT BINARY)""",
                (w, v) =>
                {
                    w.Write(v.Id, NpgsqlDbType.Uuid);
                    WriteText(w, v.Designation);
                },
                failure
            );
            var persons = new ImportTableWriter<PersonRow>(
                connectionString,
                """COPY "Persons"("Id", "LastName", "FirstName") FROM STDIN (FORMAT BINARY)""",
                (w, v) =>
                {
                    w.Write(v.Id, NpgsqlDbType.Uuid);
                    WriteText(w, v.LastName);
                    WriteText(w, v.FirstName);
                },
                failure
            );
            var links = new ImportTableWriter<LinkRow>(
                connectionString,
                """COPY "Links"("Id", "From_Id", "To_Id", "Kind", "Date") FROM STDIN (FORMAT BINARY)""",
                (w, v) =>
                {
                    w.Write(v.Id, NpgsqlDbType.Uuid);
                    w.Write(v.From, NpgsqlDbType.Uuid);
                    w.Write(v.To, NpgsqlDbType.Uuid);
                    WriteText(w, v.Kind);
                    w.Write(v.Date, NpgsqlDbType.Timestamp);
                },
                failure
            );
            var financialLinks = new ImportTableWriter<FinancialRow>(
                connectionString,
                """COPY "FinancialLinks"("Id", "Amount", "DeclarationType") FROM STDIN (FORMAT BINARY)""",
                (w, v) =>
                {
                    w.Write(v.Id, NpgsqlDbType.Uuid);
                    w.Write(v.Amount, NpgsqlDbType.Numeric);
                    WriteText(w, v.Type);
                },
                failure
            );
            const string attributeSql = """COPY "EntityAttributes"("EntityId", "Data") FROM STDIN (FORMAT BINARY)""";
            Action<NpgsqlBinaryImporter, AttributeRow> writeAttribute = (w, v) =>
            {
                w.Write(v.EntityId, NpgsqlDbType.Uuid);
                w.Write(v.Json, NpgsqlDbType.Jsonb);
            };
            var entityShards = Enumerable.Range(0, LinkAttributeWriters)
                .Select(_ => new ImportTableWriter<AttributeRow>(connectionString, attributeSql, writeAttribute, failure))
                .ToArray();
            var linkShards = Enumerable.Range(0, LinkAttributeWriters)
                .Select(_ => new ImportTableWriter<AttributeRow>(connectionString, attributeSql, writeAttribute, failure))
                .ToArray();

            var totals = new Dictionary<Guid, Totals>();
            var salt = RandomNumberGenerator.GetBytes(10);

            var producer = Task.Factory.StartNew(
                () =>
                {
                    try
                    {
                        Produce(path, scan, salt, totals, entities, companies, persons, links, financialLinks, entityShards, linkShards, progress, failure.Token);
                        entities.Complete();
                        companies.Complete();
                        persons.Complete();
                        links.Complete();
                        financialLinks.Complete();
                        foreach (var shard in entityShards.Concat(linkShards))
                        {
                            shard.Complete();
                        }
                    }
                    catch
                    {
                        failure.Cancel();
                        throw;
                    }
                },
                CancellationToken.None,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );

            var tasks = new List<Task>
            {
                producer, entities.Completion, companies.Completion, persons.Completion, links.Completion,
                financialLinks.Completion,
            };
                            tasks.AddRange(entityShards.Concat(linkShards).Select(x => x.Completion));
            await WhenAllRethrowRootCauseAsync(tasks);
            _logger.LogInformation(
                "Import: données écrites (entités {Entities}, attributs {Attributes}, liens {Links}) en {Elapsed}",
                entities.Rows,
                entityShards.Concat(linkShards).Sum(x => x.Rows),
                links.Rows,
                _total.Elapsed
            );

            progress?.Report("Totaux par entité");
            var totalRows = new ImportTableWriter<TotalRow>(
                connectionString,
                """COPY "EntityTotals"("EntityId", "LinkCount", "OutgoingAmount", "IncomingAmount", "Total") FROM STDIN (FORMAT BINARY)""",
                (w, v) =>
                {
                    w.Write(v.Id, NpgsqlDbType.Uuid);
                    w.Write(v.Count, NpgsqlDbType.Integer);
                    w.Write(v.Outgoing, NpgsqlDbType.Numeric);
                    w.Write(v.Incoming, NpgsqlDbType.Numeric);
                    w.Write(v.Outgoing + v.Incoming, NpgsqlDbType.Numeric);
                },
                failure
            );
            var totalsProducer = Task.Run(() =>
            {
                foreach (var (id, t) in totals)
                {
                    totalRows.Add(new TotalRow(id, t.Count, t.Outgoing, t.Incoming));
                }

                totalRows.Complete();
            });
            await WhenAllRethrowRootCauseAsync(new List<Task> { totalsProducer, totalRows.Completion });
            _logger.LogInformation("Import: {Rows} totaux écrits (total {Total})", totalRows.Rows, _total.Elapsed);
        }

        private void Produce(
            string path,
            ScanResult scan,
            byte[] salt,
            Dictionary<Guid, Totals> totals,
            ImportTableWriter<Guid> entities,
            ImportTableWriter<CompanyRow> companies,
            ImportTableWriter<PersonRow> persons,
            ImportTableWriter<LinkRow> links,
            ImportTableWriter<FinancialRow> financialLinks,
            ImportTableWriter<AttributeRow>[] entityShards,
            ImportTableWriter<AttributeRow>[] linkShards,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            using var file = OpenFile(path);
            using var reader = new CsvRecordReader(new StreamReader(file, new UTF8Encoding(false), true, 1 << 20));
            ReadHeader(reader);

            var keep = new bool[ExpectedColumns.Length];
            foreach (var column in new[]
            {
                Col.Id, Col.CompanyId, Col.Kind, Col.UniqueId, Col.LinkedConvention, Col.Reason, Col.OtherReason,
                Col.EventInfo, Col.Amount, Col.Date, Col.StartDate, Col.EndDate, Col.PersonId, Col.LastName,
                Col.FirstName, Col.Category, Col.PersonType, Col.PersonIdentifier, Col.Profession, Col.Structure,
                Col.Address, Col.PersonZip, Col.PersonCity, Col.Status, Col.PublicationDate, Col.CompanyName,
                Col.Siren, Col.Sector, Col.Parent, Col.CompanyCity, Col.CompanyDepartment, Col.CompanyRegion,
                Col.CompanyCountry, Col.CompanyZip, Col.Semester, Col.CountryLabel, Col.PersonCountry,
            })
            {
                keep[column] = true;
            }

            var fields = new string[ExpectedColumns.Length];
            var entitySinks = entityShards.Select(x => new AttributeSink(x)).ToArray();
            var linkSinks = linkShards.Select(x => new AttributeSink(x)).ToArray();
            var watch = Stopwatch.StartNew();
            var lastReport = watch.Elapsed;
            long row = 0;

            while (reader.ReadRecord(fields, keep))
            {
                var currentRow = row++;
                if ((currentRow & 0xFFF) == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (watch.Elapsed - lastReport > ReportInterval)
                    {
                        lastReport = watch.Elapsed;
                        Report(progress, "Chargement des données", file, watch.Elapsed, currentRow);
                    }
                }

                var companyKey = Clean(fields[Col.CompanyId]);
                var personKey = Clean(fields[Col.PersonId]);
                var linkKey = Clean(fields[Col.Id]);
                if (companyKey is null || personKey is null || linkKey is null)
                {
                    continue;
                }

                var company = scan.Companies[Hash(companyKey)];
                var entityAttributes = entitySinks[(int)(currentRow % entitySinks.Length)];
                var person = scan.Persons[Hash(personKey)];

                if (company.Row == currentRow)
                {
                    entities.Add(company.Id);
                    companies.Add(new CompanyRow(company.Id, Truncate(Clean(fields[Col.CompanyName]))));
                    AddAttribute(entityAttributes, company.Id, "Référence", companyKey);
                    AddAttribute(entityAttributes, company.Id, "SIREN", fields[Col.Siren]);
                    AddAttribute(entityAttributes, company.Id, "Secteur d'activité", fields[Col.Sector]);
                    AddAttribute(entityAttributes, company.Id, "Société mère", fields[Col.Parent]);
                    AddAttribute(entityAttributes, company.Id, "Ville", fields[Col.CompanyCity]);
                    AddAttribute(entityAttributes, company.Id, "Code postal", fields[Col.CompanyZip]);
                    AddAttribute(entityAttributes, company.Id, "Département", fields[Col.CompanyDepartment]);
                    AddAttribute(entityAttributes, company.Id, "Région", fields[Col.CompanyRegion]);
                    AddAttribute(entityAttributes, company.Id, "Pays", fields[Col.CompanyCountry]);
                }

                if (person.Row == currentRow)
                {
                    entities.Add(person.Id);
                    persons.Add(new PersonRow(person.Id, Truncate(Clean(fields[Col.LastName])), Truncate(Clean(fields[Col.FirstName]))));
                    AddAttribute(entityAttributes, person.Id, "Référence", personKey);
                    AddAttribute(entityAttributes, person.Id, "Catégorie", fields[Col.Category]);
                    AddAttribute(entityAttributes, person.Id, "Type d'identifiant", fields[Col.PersonType]);
                    AddAttribute(entityAttributes, person.Id, "Identifiant", fields[Col.PersonIdentifier]);
                    AddAttribute(entityAttributes, person.Id, "Profession", fields[Col.Profession]);
                    AddAttribute(entityAttributes, person.Id, "Structure d'exercice", fields[Col.Structure]);
                    AddAttribute(entityAttributes, person.Id, "Adresse", fields[Col.Address]?.Replace("==", ", "));
                    AddAttribute(entityAttributes, person.Id, "Code postal", fields[Col.PersonZip]);
                    AddAttribute(entityAttributes, person.Id, "Ville", fields[Col.PersonCity]);
                    AddAttribute(entityAttributes, person.Id, "Pays", Clean(fields[Col.CountryLabel]) ?? fields[Col.PersonCountry]);
                }

                if (scan.Links[Hash(linkKey)].Row != currentRow)
                {
                    continue;
                }

                var linkId = LinkGuid(currentRow, salt);
                var linkAttributes = linkSinks[(int)(currentRow % linkSinks.Length)];
                var amount = ParseAmount(fields[Col.Amount]);
                var date = SafeTimestamp(fields[Col.Date])
                    ?? SafeTimestamp(fields[Col.StartDate])
                    ?? SafeTimestamp(fields[Col.PublicationDate])
                    ?? new DateTime(1900, 1, 1);

                entities.Add(linkId);
                links.Add(new LinkRow(linkId, company.Id, person.Id, Clean(fields[Col.Kind]), date));
                financialLinks.Add(new FinancialRow(linkId, amount, DeclarationType(fields[Col.Kind])));
                AddAttribute(linkAttributes, linkId, "Référence", fields[Col.Id]);
                AddAttribute(linkAttributes, linkId, "Statut", fields[Col.Status]);
                AddAttribute(linkAttributes, linkId, "Motif", fields[Col.Reason]);
                AddAttribute(linkAttributes, linkId, "Autre motif", fields[Col.OtherReason]);
                AddAttribute(linkAttributes, linkId, "Information sur l'événement", fields[Col.EventInfo]);
                AddAttribute(linkAttributes, linkId, "Date de début", Left(fields[Col.StartDate], 10));
                AddAttribute(linkAttributes, linkId, "Date de fin", Left(fields[Col.EndDate], 10));
                AddAttribute(linkAttributes, linkId, "Date de publication", Left(fields[Col.PublicationDate], 10));
                AddAttribute(linkAttributes, linkId, "Convention liée", fields[Col.LinkedConvention]);
                AddAttribute(linkAttributes, linkId, "Identifiant unique", fields[Col.UniqueId]);
                AddAttribute(linkAttributes, linkId, "Semestre", fields[Col.Semester]);

                ref var from = ref CollectionsMarshal.GetValueRefOrAddDefault(totals, company.Id, out _);
                from.Count++;
                from.Outgoing += amount;
                ref var to = ref CollectionsMarshal.GetValueRefOrAddDefault(totals, person.Id, out _);
                to.Count++;
                to.Incoming += amount;
            }

            foreach (var sink in entitySinks.Concat(linkSinks))
            {
                sink.Flush();
            }
        }

        private async Task RebuildStructureAsync(
            string connectionString,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            progress?.Report("Création des index");
            var typeTotals = ExecuteAsync(
                connectionString,
                "Totaux par type de lien",
                new[]
                {
                    """
                    INSERT INTO "FinancialLinkTypeTotals" ("Type", "Count", "Amount")
                    SELECT COALESCE("DeclarationType", 'Non renseigné'), COUNT(*), COALESCE(SUM("Amount"), 0)
                    FROM "FinancialLinks"
                    GROUP BY 1
                    """,
                },
                progress,
                cancellationToken
            );
            using var gate = new SemaphoreSlim(MaxParallelMaintenance);
            await Task.WhenAll(
                Indexes.Select(async index =>
                {
                    await gate.WaitAsync(cancellationToken);
                    try
                    {
                        await ExecuteAsync(
                            connectionString,
                            $"Création de l'index {index.Name}",
                            new[] { "SET maintenance_work_mem = '1GB'", "SET max_parallel_maintenance_workers = 4", index.Sql },
                            progress,
                            cancellationToken
                        );
                    }
                    finally
                    {
                        gate.Release();
                    }
                })
            );

            await typeTotals;

            await ExecuteAsync(
                connectionString,
                "Création des clés étrangères",
                ForeignKeys.Select(x =>
                    $"""ALTER TABLE "{x.Table}" ADD CONSTRAINT "{x.Name}" FOREIGN KEY ("{x.Column}") REFERENCES "{x.Principal}" ("Id") ON DELETE CASCADE NOT VALID"""),
                progress,
                cancellationToken
            );

            await Task.WhenAll(
                Tables.Select(async table =>
                {
                    await gate.WaitAsync(cancellationToken);
                    try
                    {
                        await ExecuteAsync(
                            connectionString,
                            $"Statistiques de {table} (ANALYZE)",
                            new[] { $"""ANALYZE "{table}" """ },
                            progress,
                            cancellationToken
                        );
                    }
                    finally
                    {
                        gate.Release();
                    }
                })
            );
        }

        private async Task<string> GetConnectionStringAsync()
        {
            await using var db = await _contextFactory.CreateDbContextAsync();
            return db.Database.GetConnectionString()
                ?? throw new InvalidOperationException("The 'Adex' connection string is not configured.");
        }

        private async Task ExecuteAsync(
            string connectionString,
            string label,
            IEnumerable<string> statements,
            IProgress<string> progress,
            CancellationToken cancellationToken
        )
        {
            progress?.Report(label);
            //_logger.LogInformation("Import: {Label}…", label);
            var watch = Stopwatch.StartNew();
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            foreach (var sql in statements)
            {
                await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 0 };
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            _logger.LogInformation("Import: {Label} terminé en {Elapsed} (total {Total})", label, watch.Elapsed, _total.Elapsed);
        }

        private void Report(IProgress<string> progress, string label, FileStream file, TimeSpan elapsed, long rows)
        {
            var done = file.Position;
            var percent = done * 100 / Math.Max(1, file.Length);
            var perSecond = done / Math.Max(1, elapsed.TotalSeconds);
            var remaining = TimeSpan.FromSeconds((file.Length - done) / Math.Max(1, perSecond));
            var message =
                $"{label} : {percent} % ({rows:N0} lignes, {perSecond / (1 << 20):N1} Mo/s, reste ~{remaining:hh\\:mm\\:ss})";
            progress?.Report(message);
            //_logger.LogInformation("Import: {Message}", message);
        }

        private static async Task WhenAllRethrowRootCauseAsync(List<Task> tasks)
        {
            try
            {
                await Task.WhenAll(tasks);
            }
            catch
            {
                var root = tasks
                    .Where(t => t.IsFaulted)
                    .SelectMany(t => t.Exception.InnerExceptions)
                    .FirstOrDefault(e => e is not OperationCanceledException);
                if (root is not null)
                {
                    ExceptionDispatchInfo.Capture(root).Throw();
                }

                throw;
            }
        }

        private static FileStream OpenFile(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
        }

        private static void ReadHeader(CsvRecordReader reader)
        {
            var header = new string[ExpectedColumns.Length];
            var all = Enumerable.Repeat(true, ExpectedColumns.Length).ToArray();
            if (!reader.ReadRecord(header, all))
            {
                throw new InvalidDataException("declarations.csv is empty.");
            }

            header[0] = header[0].TrimStart('\uFEFF');
            if (!header.SequenceEqual(ExpectedColumns, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidDataException("Unexpected declarations.csv header.");
            }
        }

        private static void WriteText(NpgsqlBinaryImporter writer, string value)
        {
            if (value is null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.Write(value, NpgsqlDbType.Text);
            }
        }

        private static void AddAttribute(AttributeSink sink, Guid entityId, string name, string value)
        {
            var cleaned = Clean(value);
            if (cleaned is not null)
            {
                sink.Add(entityId, name, cleaned);
            }
        }

        private static string Clean(string value)
        {
            if (value is null)
            {
                return null;
            }

            var trimmed = value.Trim(' ');
            return trimmed.Length == 0 ? null : trimmed;
        }

        private static string Left(string value, int length)
        {
            return value is not null && value.Length > length ? value[..length] : value;
        }

        // varchar(200) limits characters; never cut a surrogate pair in half.
        private static string Truncate(string value)
        {
            if (value is null || value.Length <= 200)
            {
                return value;
            }

            var end = char.IsHighSurrogate(value[199]) ? 199 : 200;
            return value[..end];
        }

        // 64-bit FNV-1a: sources keys are replaced by their hash to keep the pick tables small.
        private static long Hash(ReadOnlySpan<char> value)
        {
            var hash = 14695981039346656037UL;
            foreach (var c in value)
            {
                hash = (hash ^ c) * 1099511628211UL;
            }

            return unchecked((long)hash);
        }

        // Sortable number built from the first 14 digits; blank dates sort last, as NULLS LAST did.
        private static long DateKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return -1;
            }

            long key = 0;
            var digits = 0;
            foreach (var c in value)
            {
                if (c >= '0' && c <= '9' && digits < 14)
                {
                    key = key * 10 + (c - '0');
                    digits++;
                }
            }

            for (; digits < 14; digits++)
            {
                key *= 10;
            }

            return key;
        }

        // Time-ordered ids built from the line number: unique, and sequential for the primary key index.
        private static Guid LinkGuid(long row, byte[] salt)
        {
            Span<byte> bytes = stackalloc byte[16];
            for (var i = 0; i < 6; i++)
            {
                bytes[i] = (byte)(row >> (8 * (5 - i)));
            }

            bytes[6] = (byte)(0x70 | (salt[0] & 0x0F));
            bytes[7] = salt[1];
            bytes[8] = (byte)(0x80 | (salt[2] & 0x3F));
            for (var i = 9; i < 16; i++)
            {
                bytes[i] = salt[i - 6];
            }

            return new Guid(bytes, true);
        }

        private static DateTime? SafeTimestamp(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 10)
            {
                return null;
            }

            var text = value.Length > 19 ? value[..19] : value;
            text = text.Replace('T', ' ');
            return DateTime.TryParseExact(text, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : null;
        }

        private static decimal ParseAmount(string value)
        {
            var text = Clean(value)?.Replace(',', '.');
            if (text is null)
            {
                return 0;
            }

            var start = text[0] == '-' ? 1 : 0;
            var dot = -1;
            for (var i = start; i < text.Length; i++)
            {
                if (text[i] == '.' && dot < 0 && i > start && i < text.Length - 1)
                {
                    dot = i;
                }
                else if (text[i] < '0' || text[i] > '9')
                {
                    return 0;
                }
            }

            return text.Length > start
                && decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var amount)
                ? amount
                : 0;
        }

        private static string DeclarationType(string kind)
        {
            return Clean(kind)?.ToLowerInvariant() switch
            {
                "avantage" => "Avantage",
                "convention" => "Convention",
                "remuneration" => "Rémunération",
                "rémunération" => "Rémunération",
                _ => "Non renseigné",
            };
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

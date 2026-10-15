using System;
using System.Threading;
using System.Threading.Tasks;
using Adex.Application.Import;
using Microsoft.Extensions.Logging;

namespace Adex.Importer
{
    public class ImportRunner(
        IDeclarationsImporter importer,
        ImportPreflight preflight,
        ILogger<ImportRunner> logger
    )
    {
        /// <summary>
        /// Vérifie la configuration, demande confirmation puis exécute l'import complet.
        /// Retourne le code de sortie du processus.
        /// </summary>
        public async Task<int> RunAsync(bool assumeYes, CancellationToken cancellationToken)
        {
            try
            {
                var path = await preflight.CheckAsync(cancellationToken);
                if (path is null)
                {
                    logger.LogError("Vérifications en échec, l'import n'est pas lancé");
                    return 1;
                }

                if (!assumeYes && !await ConfirmAsync(cancellationToken))
                {
                    logger.LogInformation("Import non lancé par l'utilisateur");
                    return 0;
                }

                logger.LogInformation("Full import of {Path} started", path);

                var result = await importer.ImportAsync(
                    path,
                    new Progress<string>(message => logger.LogInformation("Import: {Message}", message)),
                    cancellationToken
                );

                logger.LogInformation(
                    "Import finished: {Rows} lignes lues, {Companies} entreprises, "
                        + "{Beneficiaries} bénéficiaires, {Links} liens financiers, "
                        + "{Skipped} lignes ignorées",
                    result.Rows,
                    result.Companies,
                    result.Beneficiaries,
                    result.Links,
                    result.SkippedRows
                );
                return result.SkippedRows == 0 ? 0 : 2;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning("Import cancelled, database may be partial, rerun the import");
                return 130;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Import failed, database may be partial, rerun the import");
                return 1;
            }
        }

        private static async Task<bool> ConfirmAsync(CancellationToken cancellationToken)
        {
            Console.Write("L'import remplace les données existantes. Lancer l'import ? (o/N) ");
            var answer = await Task.Run(Console.ReadLine, CancellationToken.None)
                .WaitAsync(cancellationToken);
            return answer?.Trim().ToLowerInvariant() is "o" or "oui" or "y" or "yes";
        }
    }
}

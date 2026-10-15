using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Adex.Importer
{
    /// <summary>
    /// Affiche la configuration d'import et vérifie la base de données et le fichier de données.
    /// </summary>
    public class ImportPreflight(
        NpgsqlDataSource dataSource,
        IHostEnvironment environment,
        IOptions<ImportOptions> options
    )
    {
        private const int PreviewLineCount = 10;
        private const int MaxLineLength = 200;

        /// <summary>Retourne le chemin du fichier à importer, ou null si une vérification échoue.</summary>
        public async Task<string> CheckAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("=== Configuration de l'import ===");
            Console.WriteLine($"Environnement : {environment.EnvironmentName}");

            var databaseOk = await CheckDatabaseAsync(cancellationToken);
            var path = await CheckFileAsync(cancellationToken);
            Console.WriteLine();
            return databaseOk ? path : null;
        }

        private async Task<bool> CheckDatabaseAsync(CancellationToken cancellationToken)
        {
            var builder = new NpgsqlConnectionStringBuilder(dataSource.ConnectionString);
            Console.WriteLine(
                $"Base de données : {builder.Host}:{builder.Port}/{builder.Database} (utilisateur {builder.Username})"
            );
            try
            {
                await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
                Console.WriteLine($"  Connexion : OK (PostgreSQL {connection.PostgreSqlVersion})");
                return true;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Console.WriteLine($"  Connexion : ÉCHEC - {e.Message}");
                return false;
            }
        }

        // Nombre de sauts de ligne : une valeur CSV multiligne serait comptée plusieurs fois.
        private static async Task<long> CountLinesAsync(
            string path,
            CancellationToken cancellationToken
        )
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                1 << 20,
                FileOptions.SequentialScan | FileOptions.Asynchronous
            );
            var buffer = new byte[1 << 20];
            long lines = 0;
            byte last = (byte)'\n';
            int read;
            while ((read = await stream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                lines += buffer.AsSpan(0, read).Count((byte)'\n');
                last = buffer[read - 1];
            }

            return last == (byte)'\n' ? lines : lines + 1;
        }

        private async Task<string> CheckFileAsync(CancellationToken cancellationToken)
        {
            var value = options.Value;
            var directory = Path.GetFullPath(value.DataDirectory, environment.ContentRootPath);
            var path = Path.Combine(directory, Path.GetFileName(value.FileName));
            Console.WriteLine($"Fichier de données : {path}");

            try
            {
                var info = new FileInfo(path);
                if (!info.Exists)
                {
                    Console.WriteLine("  Lecture : ÉCHEC - le fichier n'existe pas");
                    return null;
                }

                using var reader = new StreamReader(path);
                var count = 0;
                while (
                    count < PreviewLineCount
                    && await reader.ReadLineAsync(cancellationToken) is { } line
                )
                {
                    if (count == 0)
                    {
                        Console.WriteLine("  Lecture : OK, premières lignes :");
                    }

                    count++;
                    Console.WriteLine(
                        $"  {count,2}| {(line.Length > MaxLineLength ? line[..MaxLineLength] + "…" : line)}"
                    );
                }

                if (count == 0)
                {
                    Console.WriteLine("  Lecture : ÉCHEC - le fichier est vide");
                    return null;
                }

                Console.WriteLine($"  Taille : {info.Length / 1024d / 1024d:N1} Mo");
                Console.Write("  Comptage des lignes en cours... ");
                var lines = await CountLinesAsync(path, cancellationToken);
                Console.WriteLine($"\r  Lignes à importer : {Math.Max(0, lines - 1):N0} (hors en-tête)");
                return path;
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                Console.WriteLine($"  Lecture : ÉCHEC - {e.Message}");
                return null;
            }
        }
    }
}

using Adex.Business;
using Adex.Common;
using Adex.WebApi.Explorer;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Import
{
    public class ImportJobService
    {
        private static readonly string[] LinkFilePatterns =
        {
            "declaration_avantage_*.csv",
            "declaration_convention_*.csv",
            "declaration_remuneration_*.csv",
        };

        private const string ProviderFilePattern = "entreprise_*.csv";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IWebHostEnvironment _environment;
        private readonly ImportOptions _options;
        private readonly ILogger<ImportJobService> _logger;
        private readonly object _sync = new();

        private ImportStatus _status = new();
        private CancellationTokenSource _cancellation;

        public ImportJobService(
            IServiceScopeFactory scopeFactory,
            IHostApplicationLifetime lifetime,
            IWebHostEnvironment environment,
            IOptions<ImportOptions> options,
            ILogger<ImportJobService> logger
        )
        {
            _scopeFactory = scopeFactory;
            _lifetime = lifetime;
            _environment = environment;
            _options = options.Value;
            _logger = logger;
        }

        public bool IsEnabled => !string.IsNullOrEmpty(_options.ApiKey);

        public bool IsAuthorized(string apiKey)
        {
            if (!IsEnabled || string.IsNullOrEmpty(apiKey))
            {
                return false;
            }

            var expected = System.Text.Encoding.UTF8.GetBytes(_options.ApiKey);
            var actual = System.Text.Encoding.UTF8.GetBytes(apiKey);
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                expected,
                actual
            );
        }

        public ImportStatus GetStatus()
        {
            lock (_sync)
            {
                return Snapshot(_status);
            }
        }

        public bool TryStart(ImportTarget target, out ImportStatus status)
        {
            lock (_sync)
            {
                if (_status.State == ImportState.Running)
                {
                    status = Snapshot(_status);
                    return false;
                }

                _cancellation?.Dispose();
                _cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                    _lifetime.ApplicationStopping
                );
                _status = new ImportStatus
                {
                    State = ImportState.Running,
                    Target = target,
                    StartedAt = DateTimeOffset.UtcNow,
                };
                if (target != ImportTarget.Normalized)
                {
                    _status.Steps.Add(
                        new ImportStepStatus { Name = nameof(ImportTarget.Metadata) }
                    );
                }

                if (target != ImportTarget.Metadata)
                {
                    _status.Steps.Add(new ImportStepStatus { Name = nameof(ImportTarget.Normalized) });
                }

                if (target == ImportTarget.All)
                {
                    _status.Steps.Reverse();
                }

                var token = _cancellation.Token;
                var run = _status;
                status = Snapshot(run);
                _ = Task.Run(() => RunAsync(run, token), CancellationToken.None);
                return true;
            }
        }

        public bool Cancel()
        {
            lock (_sync)
            {
                if (_status.State != ImportState.Running)
                {
                    return false;
                }

                _cancellation.Cancel();
                return true;
            }
        }

        private async Task RunAsync(ImportStatus run, CancellationToken cancellationToken)
        {
            try
            {
                var files = run.Steps.Any(step => step.Name == nameof(ImportTarget.Normalized))
                    ? ResolveFiles()
                    : null;
                foreach (var step in run.Steps.ToList())
                {
                    await RunStepAsync(step, files, cancellationToken);
                }

                lock (_sync)
                {
                    run.State = run.Steps.All(x => x.State == ImportState.Completed)
                        ? ImportState.Completed
                        : run.Steps.Any(x => x.State == ImportState.Cancelled)
                            ? ImportState.Cancelled
                            : run.Steps.All(x => x.State == ImportState.Failed)
                                ? ImportState.Failed
                                : ImportState.CompletedWithErrors;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "The import could not be started");
                lock (_sync)
                {
                    foreach (var step in run.Steps)
                    {
                        step.State = ImportState.Failed;
                        step.FailureMessage = e.Message;
                    }

                    run.State = ImportState.Failed;
                }
            }
            finally
            {
                lock (_sync)
                {
                    run.FinishedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        private async Task RunStepAsync(
            ImportStepStatus step,
            ImportFiles files,
            CancellationToken cancellationToken
        )
        {
            lock (_sync)
            {
                step.State = ImportState.Running;
            }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                if (step.Name == nameof(ImportTarget.Metadata))
                {
                    var transferredCount = await scope.ServiceProvider
                        .GetRequiredService<MetadataConsolidationService>()
                        .ConsolidateAsync(cancellationToken);

                    lock (_sync)
                    {
                        step.State = ImportState.Completed;
                    }

                    _logger.LogInformation(
                        "Consolidated {MetadataCount} metadata records into the normalized database",
                        transferredCount
                    );
                    return;
                }

                ICsvLoader loader = scope.ServiceProvider.GetRequiredService<CsvLoaderNormalized>();
                loader.OnMessage += (_, message) =>
                {
                    if (message.Level == Level.Error)
                    {
                        lock (_sync)
                        {
                            step.ErrorCount++;
                        }
                    }
                };

                _logger.LogInformation("Import step {Step} started", step.Name);
                await loader.LoadReferencesAsync(cancellationToken);
                await loader.LoadProvidersAsync(files.Providers, cancellationToken);
                foreach (var linkFile in files.Links)
                {
                    await loader.LoadLinksAsync(linkFile, cancellationToken);
                }

                if (step.Name == nameof(ImportTarget.Normalized))
                {
                    await loader.SaveAsync(cancellationToken);
                }

                lock (_sync)
                {
                    step.State = step.ErrorCount == 0
                        ? ImportState.Completed
                        : ImportState.CompletedWithErrors;
                }

                _logger.LogInformation(
                    "Import step {Step} finished with {ErrorCount} errors",
                    step.Name,
                    step.ErrorCount
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                lock (_sync)
                {
                    step.State = ImportState.Cancelled;
                }

                _logger.LogWarning("Import step {Step} cancelled", step.Name);
            }
            catch (Exception e)
            {
                lock (_sync)
                {
                    step.State = ImportState.Failed;
                    step.FailureMessage = e.Message;
                }

                _logger.LogError(e, "Import step {Step} failed", step.Name);
            }
        }

        private ImportFiles ResolveFiles()
        {
            if (string.IsNullOrWhiteSpace(_options.DataDirectory))
            {
                throw new InvalidOperationException("Import:DataDirectory is not configured.");
            }

            var directory = Path.GetFullPath(_options.DataDirectory, _environment.ContentRootPath);
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Import directory '{directory}' does not exist.");
            }

            return new ImportFiles(
                FindLatest(directory, ProviderFilePattern),
                LinkFilePatterns.Select(x => FindLatest(directory, x)).ToList()
            );
        }

        private static string FindLatest(string directory, string pattern)
        {
            return Directory
                    .EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly)
                    .OrderByDescending(Path.GetFileName, StringComparer.Ordinal)
                    .FirstOrDefault()
                ?? throw new FileNotFoundException(
                    $"No file matching '{pattern}' in '{directory}'."
                );
        }

        private static ImportStatus Snapshot(ImportStatus source)
        {
            return new ImportStatus
            {
                State = source.State,
                Target = source.Target,
                StartedAt = source.StartedAt,
                FinishedAt = source.FinishedAt,
                Steps = source
                    .Steps.Select(x => new ImportStepStatus
                    {
                        Name = x.Name,
                        State = x.State,
                        ErrorCount = x.ErrorCount,
                        FailureMessage = x.FailureMessage,
                    })
                    .ToList(),
            };
        }

        private sealed record ImportFiles(string Providers, IReadOnlyList<string> Links);
    }
}

using Adex.Business;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Adex.WebApi.Import
{
    public class ImportJobService
    {
        private const string StepName = "Declarations";

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IWebHostEnvironment _environment;
        private readonly ImportOptions _options;
        private readonly ILogger<ImportJobService> _logger;
        private readonly SemaphoreSlim _sync = new(1, 1);

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

        public bool IsRunning => GetStatus().State == ImportState.Running;

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
            using (Acquire())
            {
                return Snapshot(_status);
            }
        }

        public bool TryStart(out ImportStatus status)
        {
            using (Acquire())
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
                    StartedAt = DateTimeOffset.UtcNow,
                };
                _status.Steps.Add(new ImportStepStatus { Name = StepName });

                var token = _cancellation.Token;
                var run = _status;
                status = Snapshot(run);
                _ = Task.Run(() => RunAsync(run, token), CancellationToken.None);
                return true;
            }
        }

        public bool Cancel()
        {
            using (Acquire())
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
            var step = run.Steps.Single();
            try
            {
                using (Acquire())
                {
                    step.State = ImportState.Running;
                }

                var path = ResolveFile();
                _logger.LogInformation("Full import of {Path} started", path);

                await using var scope = _scopeFactory.CreateAsyncScope();
                var cache = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                var result = await scope.ServiceProvider
                    .GetRequiredService<DeclarationsImporter>()
                    .ImportAsync(
                        path,
                        new Progress<string>(message =>
                        {
                            using (Acquire())
                            {
                                step.Message = message;
                            }

                            _logger.LogInformation("Import: {Message}", message);
                        }),
                        cancellationToken
                    );

                cache.Remove(Explorer.DataExplorerService.DashboardCacheKey);

                using (Acquire())
                {
                    step.ErrorCount = (int)Math.Min(result.SkippedRows, int.MaxValue);
                    step.Message =
                        $"{result.Rows} lignes lues, {result.Companies} entreprises, "
                        + $"{result.Beneficiaries} bénéficiaires, {result.Links} liens financiers, "
                        + $"{result.SkippedRows} lignes ignorées";
                    step.State = step.ErrorCount == 0
                        ? ImportState.Completed
                        : ImportState.CompletedWithErrors;
                    run.State = step.State;
                }

                _logger.LogInformation("Import finished: {Summary}", step.Message);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                using (Acquire())
                {
                    step.State = ImportState.Cancelled;
                    run.State = ImportState.Cancelled;
                }

                _logger.LogWarning("Import cancelled, database may be partial, rerun the import");
            }
            catch (Exception e)
            {
                using (Acquire())
                {
                    step.State = ImportState.Failed;
                    step.FailureMessage = e.Message;
                    run.State = ImportState.Failed;
                }

                _logger.LogError(e, "Import failed, database may be partial, rerun the import");
            }
            finally
            {
                using (Acquire())
                {
                    run.FinishedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        private IDisposable Acquire()
        {
            _sync.Wait();
            return new Releaser(_sync);
        }

        private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
        {
            public void Dispose() => semaphore.Release();
        }

        private string ResolveFile()
        {
            if (string.IsNullOrWhiteSpace(_options.DataDirectory))
            {
                throw new InvalidOperationException("Import:DataDirectory is not configured.");
            }

            var directory = Path.GetFullPath(_options.DataDirectory, _environment.ContentRootPath);
            var path = Path.Combine(directory, Path.GetFileName(_options.FileName));
            return File.Exists(path)
                ? path
                : throw new FileNotFoundException($"Import file '{path}' does not exist.");
        }

        private static ImportStatus Snapshot(ImportStatus source)
        {
            return new ImportStatus
            {
                State = source.State,
                StartedAt = source.StartedAt,
                FinishedAt = source.FinishedAt,
                Steps = source
                    .Steps.Select(x => new ImportStepStatus
                    {
                        Name = x.Name,
                        State = x.State,
                        ErrorCount = x.ErrorCount,
                        FailureMessage = x.FailureMessage,
                        Message = x.Message,
                    })
                    .ToList(),
            };
        }
    }
}

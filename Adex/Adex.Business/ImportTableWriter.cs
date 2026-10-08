using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Npgsql;

namespace Adex.Business
{
    /// <summary>
    /// Feeds one table through a dedicated connection with a binary COPY. The producer thread
    /// adds rows; batches are handed to a background task that streams them to PostgreSQL.
    /// </summary>
    internal sealed class ImportTableWriter<T>
    {
        private const int BatchSize = 1024;

        private readonly Channel<(T[] Items, int Count)> _channel = Channel.CreateBounded<(T[], int)>(
            new BoundedChannelOptions(16)
            {
                SingleReader = true,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            }
        );

        private readonly CancellationToken _token;
        private T[] _batch = new T[BatchSize];
        private int _count;

        public ImportTableWriter(
            string connectionString,
            string copySql,
            Action<NpgsqlBinaryImporter, T> write,
            CancellationTokenSource failure
        )
        {
            _token = failure.Token;
            Completion = Task.Run(async () =>
            {
                try
                {
                    await RunAsync(connectionString, copySql, write);
                }
                catch
                {
                    failure.Cancel();
                    throw;
                }
            });
        }

        public Task Completion { get; }

        public long Rows { get; private set; }

        public void Add(in T item)
        {
            _batch[_count++] = item;
            if (_count == _batch.Length)
            {
                Flush();
            }
        }

        public void Complete()
        {
            Flush();
            _channel.Writer.TryComplete();
        }

        private void Flush()
        {
            if (_count == 0)
            {
                return;
            }

            var full = (_batch, _count);
            _batch = new T[BatchSize];
            _count = 0;
            _channel.Writer.WriteAsync(full, _token).AsTask().GetAwaiter().GetResult();
        }

        private async Task RunAsync(string connectionString, string copySql, Action<NpgsqlBinaryImporter, T> write)
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(_token);
            await using (var command = new NpgsqlCommand("SET synchronous_commit = off", connection))
            {
                await command.ExecuteNonQueryAsync(_token);
            }

            await using var importer = await connection.BeginBinaryImportAsync(copySql, _token);
            await foreach (var (items, count) in _channel.Reader.ReadAllAsync(_token))
            {
                for (var i = 0; i < count; i++)
                {
                    importer.StartRow();
                    write(importer, items[i]);
                }

                Rows += count;
            }

            await importer.CompleteAsync(_token);
        }
    }
}

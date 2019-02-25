using System;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace SteemDataScraper.DataAccess
{
    public static class TransferAccessor
    {
        public static async Task CreateTransferPartitionIfNotExist(this NpgsqlConnection connection, DateTime month, CancellationToken token)
        {
            var isExists = await IsTransferPartitionExist(connection, month, token);
            if (isExists)
                return;

            var cmd = $@"CREATE TABLE public.transfer_{month:yyyy_MM} PARTITION OF transfer FOR VALUES FROM ('{month:yyyy-MM}-01') TO ('{month.AddMonths(1):yyyy-MM}-01');";

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            await command.ExecuteNonQueryAsync(token);
        }

        public static async Task<bool> IsTransferPartitionExist(this NpgsqlConnection connection, DateTime month, CancellationToken token)
        {
            var cmd = $"SELECT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname = 'public' AND tablename = 'transfer_{month:yyyy_MM}');";

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            var obj = await command.ExecuteScalarAsync(token);
            return obj != null && (bool)obj;
        }
    }
}
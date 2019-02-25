using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.DataAccess
{
    public static class SystemAccessor
    {
        public static async Task<List<string>> GetVersionNamesAsync(this NpgsqlConnection connection, CancellationToken token)
        {
            var cmd = "SELECT name FROM public.version;";
            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            var result = new List<string>();

            using (var reader = await command.ExecuteReaderAsync(token))
            {
                while (reader.Read())
                {
                    result.Add(reader.GetValueOrDefult<string>(0));
                }
            }
            return result;
        }

        public static Task UpdateDb(this NpgsqlConnection connection, string sql, CancellationToken token)
        {
            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = sql
            };

            return command.ExecuteNonQueryAsync(token);
        }

        public static Task AddVersion(this NpgsqlConnection connection, string fileName, CancellationToken token)
        {
            var cmd = $"INSERT INTO public.version(name) VALUES ('{fileName}');";
            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            return command.ExecuteNonQueryAsync(token);
        }



        public static async Task<T> GetServiceStateAsync<T>(this NpgsqlConnection connection, int serviceId, CancellationToken token)
        {
            var cmd = $"SELECT json FROM public.service_state WHERE service_id = {serviceId};";
            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            var result = await command.ExecuteScalarAsync(token);
            if (result is DBNull || result is null)
                return default(T);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>((string)result);
        }

        public static Task UpdateServiceStateAsync(this NpgsqlConnection connection, int serviceId, string json, CancellationToken token)
        {
            var cmd = $"UPDATE public.service_state SET json = @p1 WHERE service_id = {serviceId};";
            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            command.Parameters.AddValue("@p1", json);
            return command.ExecuteNonQueryAsync(token);
        }
    }
}
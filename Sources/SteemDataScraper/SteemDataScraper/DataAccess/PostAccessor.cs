using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using SteemDataScraper.Extensions;
using SteemDataScraper.Models;

namespace SteemDataScraper.DataAccess
{
    public static class PostAccessor
    {
        public static async Task<bool> IsNodeInfoExist(this NpgsqlConnection connection, CancellationToken token)
        {
            var cmd = "SELECT count(*) FROM public.node_info;";

            var command = new NpgsqlCommand
            {
                Connection = connection,
                CommandText = cmd
            };

            var result = await command.ExecuteScalarAsync(token);
            if (result is DBNull || result == null)
                return false;
            return (long)result > 0;
        }
    }
}

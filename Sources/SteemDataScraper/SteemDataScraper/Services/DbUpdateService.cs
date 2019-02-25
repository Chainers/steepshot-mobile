using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Npgsql;
using SteemDataScraper.DataAccess;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.Services
{
    public class DbUpdateService : BaseDbService
    {
        private IFileProvider _fileProvider;

        protected override bool SingleRun => true;

        public DbUpdateService(ILogger<DbUpdateService> logger, IConfiguration configuration, IFileProvider fileProvider)
            : base(logger, configuration)
        {
            _fileProvider = fileProvider;
        }


        protected override async Task DoSomethingAsync(NpgsqlConnection connection, CancellationToken token)
        {
            NpgsqlTransaction transaction = null;

            try
            {
                var nameFormat = new Regex(@"^\d{4}-\d{2}-\d{2}-\d{2}.sql$");
                var contents = _fileProvider
                    .GetDirectoryContents("sql")
                    .Where(f => nameFormat.IsMatch(f.Name))
                    .OrderBy(f => f.Name)
                    .ToArray();

                if (!contents.Any())
                    return;

                var versions = await connection.GetVersionNamesAsync(token);
                transaction = connection.BeginTransaction();

                foreach (var content in contents)
                {
                    if (versions.Contains(content.Name))
                        continue;

                    string sql;
                    using (var sr = new StreamReader(content.CreateReadStream()))
                    {
                        sql = sr.ReadToEnd();
                    }
                    await connection.UpdateDb(sql, token);
                    await connection.AddVersion(content.Name, token);
                }

                transaction.CommitAndDispose();
            }
            catch (OperationCanceledException)
            {
                transaction.RollbackAndDispose();
            }
            catch (Exception)
            {
                transaction.RollbackAndDispose();
                throw;
            }
        }
    }
}

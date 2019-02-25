using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SteemDataScraper.Services
{
    public abstract class BaseDbService : IHostedService, IDisposable
    {
        private bool _preventDdos;
        private Task _executingTask;
        protected CancellationTokenSource StoppingCts;


        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        protected readonly object Sync = new object();
        protected CancellationToken Token;
        protected virtual bool SingleRun => false;
        protected virtual bool OpenConnect => true;

        protected BaseDbService(ILogger logger, IConfiguration configuration)
        {
            Logger = logger;
            Configuration = configuration;
        }

        public async Task StartAndWaitAsync(CancellationToken cancellationToken)
        {
            if (_executingTask != null)
                return;

            lock (Sync)
            {
                if (_executingTask != null)
                    return;

                Logger.LogInformation($"{GetType().Name} is starting.");

                // Store the task we're executing
                StoppingCts = new CancellationTokenSource();
            }

            await ExecuteAsync(StoppingCts.Token);
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            if (_executingTask != null)
                return Task.CompletedTask;

            lock (Sync)
            {
                if (_executingTask != null)
                    return Task.CompletedTask;

                Logger.LogInformation($"{GetType().Name} is starting.");

                // Store the task we're executing
                StoppingCts = new CancellationTokenSource();
                _executingTask = ExecuteAsync(StoppingCts.Token);

                // If the task is completed then return it, 
                // this will bubble cancellation and failure to the caller
                if (_executingTask.IsCompleted)
                {
                    return _executingTask;
                }

                // Otherwise it's running
                return Task.CompletedTask;
            }
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executingTask == null || StoppingCts == null || StoppingCts.IsCancellationRequested)
                return;

            lock (Sync)
            {
                if (_executingTask == null || StoppingCts == null || StoppingCts.IsCancellationRequested)
                    return;

                Logger.LogInformation($"{GetType().Name} is stopping.");
                // Signal cancellation to the executing method
                StoppingCts.Cancel();
            }

            // Wait until the task completes or the stop token triggers
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken))
                .ConfigureAwait(false);
            _executingTask = null;
            StoppingCts = null;

            Logger.LogInformation($"{GetType().Name} stopped");
        }

        public virtual void Dispose()
        {
            StoppingCts.Cancel();
        }

        protected virtual async Task ExecuteAsync(CancellationToken token)
        {
            Token = token;
            Logger.LogInformation($"{GetType().Name} started");

            while (!token.IsCancellationRequested)
            {
                NpgsqlConnection connection = null;

                try
                {
                    if (_preventDdos)
                        await Task.Delay(TimeSpan.FromMinutes(5), token);

                    var connectionString = Configuration.GetConnectionString("DefaultConnection");
                    connection = new NpgsqlConnection(connectionString);
                    if (OpenConnect)
                        connection.Open();

                    await DoSomethingAsync(connection, token);

                    _preventDdos = false;
                    if (SingleRun)
                        break;
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _preventDdos = true;
                    Logger.LogError(ex, ex.Message);
                }
                finally
                {
                    if (connection != null)
                    {
                        if (connection.State == ConnectionState.Open)
                            connection.Close();
                        connection.Dispose();
                    }
                }
            }
        }

        protected abstract Task DoSomethingAsync(NpgsqlConnection connection, CancellationToken token);
    }
}
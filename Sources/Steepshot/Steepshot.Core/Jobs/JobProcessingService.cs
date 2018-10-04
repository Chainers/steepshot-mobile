using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Jobs.Upload;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Jobs
{
    public class JobProcessingService
    {
        private readonly Dictionary<int, ICommand> _commands;
        private readonly DbManager _dbService;
        private readonly ILogService _logService;
        private int _isStopped = 1;

        private Job _currentJob;

        public JobProcessingService(SteepshotClient steepshotClient,
            SteepshotApiClient steepshotSteemApiClient, SteepshotApiClient steepshotGolosApiClient,
            SteemClient steemClient, GolosClient golosClient,
            IConnectionService connectionService, ILogService logService,
            DbManager dbService, UserManager userManager, IFileProvider fileProvider)
        {
            _dbService = dbService;
            _logService = logService;

            _commands = new Dictionary<int, ICommand>
            {
                {
                    UploadMediaCommand.Id,
                    new UploadMediaCommand(steepshotClient, steepshotSteemApiClient, steepshotGolosApiClient,
                        connectionService, logService,
                        userManager, dbService, fileProvider)
                }
            };
        }

        public Task StartAsync()
        {
            return Task.Run(Start);
        }

        public async Task Start()
        {
            if (Interlocked.CompareExchange(ref _isStopped, 0, 1) == 0)
                return;

            int curId = 0;
            while (true)
            {
                if (_isStopped == 1)
                    break;

                try
                {
                    CancellationToken token;

                    lock (_dbService)
                    {
                        var id = curId;
                        var jobs = _dbService
                            .SelectTable<Job>()
                            .Where(j => j.Id > id && j.State != JobState.Failed && j.State != JobState.Ready)
                            .OrderBy(job => job.LastStartTime);

                        _currentJob = jobs.FirstOrDefault();

                        if (_currentJob != null)
                        {
                            _currentJob.CancellationTokenSource = new CancellationTokenSource();
                            token = _currentJob.CancellationTokenSource.Token;

                            if (_isStopped == 1)
                                break;
                        }
                        else
                        {
                            if (curId == 0 && !jobs.Any())
                                break;

                            curId = 0;
                            continue;
                        }
                    }

                    curId = _currentJob.Id;
                    token.ThrowIfCancellationRequested();

                    var cmd = _commands[_currentJob.CommandId];
                    _currentJob.LastStartTime = DateTime.Now;
                    _currentJob.State = await cmd.Execute(_currentJob.DataId, token);

                    token.ThrowIfCancellationRequested();

                    lock (_dbService)
                        _dbService.Update(_currentJob);

                    _currentJob = null;
                }
                catch (OperationCanceledException)
                {
                    //todo nothing
                }
                catch (Exception ex)
                {
                    await _logService.ErrorAsync(ex);

                    if (_currentJob != null)
                    {
                        _currentJob.State = JobState.Failed;
                        lock (_dbService)
                            _dbService.Update(_currentJob);
                    }
                }
            }

            Interlocked.CompareExchange(ref _isStopped, 1, 0);
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _isStopped, 1, 0) == 1)
                return;

            var cJob = _currentJob;
            if (cJob != null && !cJob.CancellationTokenSource.IsCancellationRequested)
                cJob.CancellationTokenSource.Cancel();
        }

        public void DeleteJob(int jobId)
        {
            var cJob = _currentJob;
            if (cJob != null && cJob.Id == jobId && !cJob.CancellationTokenSource.IsCancellationRequested)
                cJob.CancellationTokenSource.Cancel();

            lock (_dbService)
            {
                var job = _dbService.Select<Job>(jobId);
                _dbService.Delete<Job>(jobId);
                _commands[job.CommandId].CleanData(job.DataId);
            }
        }

        public void AddJob<T>(Job job, T data) where T : SqlTableBase
        {
            lock (_dbService)
            {
                _dbService.Insert(data);

                job.DataId = data.Id;
                job.State = JobState.Added;
                _dbService.Insert(job);
            }
        }

        public JobState GetJobState(int jobId)
        {
            lock (_dbService)
            {
                var job = _dbService.Select<Job>(jobId);
                return job.State;
            }
        }

        public object GetResult(int jobId)
        {
            lock (_dbService)
            {
                var job = _dbService.Select<Job>(jobId);
                return _commands[job.CommandId].GetResult(job.DataId);
            }
        }
    }
}

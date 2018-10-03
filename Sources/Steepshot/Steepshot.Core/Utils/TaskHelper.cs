using System;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Utils
{
    public class TaskHelper
    {
        private readonly IConnectionService _connectionService;
        private readonly ILogService _logService;

        public TaskHelper(IConnectionService connectionService, ILogService logService)
        {
            _connectionService = connectionService;
            _logService = logService;
        }

        #region TryRunTaskAsync


        public async Task<OperationResult<TResult>> TryRunTaskAsync<TResult>(Func<CancellationToken, Task<OperationResult<TResult>>> func, CancellationToken ct)
        {
            try
            {
                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await _logService.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }

        public async Task<OperationResult<TResult>> TryRunTaskAsync<T1, TResult>(Func<T1, CancellationToken, Task<OperationResult<TResult>>> func, T1 arg1, CancellationToken ct)
        {
            try
            {
                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(arg1, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await _logService.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }

        public async Task<OperationResult<TResult>> TryRunTaskAsync<T1, T2, TResult>(Func<T1, T2, CancellationToken, Task<OperationResult<TResult>>> func, T1 arg1, T2 arg2, CancellationToken ct)
        {
            try
            {
                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                return await func(arg1, arg2, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested)
                    return new OperationResult<TResult>(new OperationCanceledException());

                var available = _connectionService.IsConnectionAvailable();
                if (!available)
                    return new OperationResult<TResult>(new ValidationException(LocalizationKeys.InternetUnavailable));

                await _logService.ErrorAsync(ex).ConfigureAwait(false);

                return new OperationResult<TResult>(ex);
            }
        }
        
        #endregion
    }
}

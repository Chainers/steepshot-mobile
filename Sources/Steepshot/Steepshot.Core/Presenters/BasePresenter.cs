using System.Threading;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Presenters
{
    public abstract class BasePresenter
    {
        public CancellationTokenSource OnDisposeCts { get; private set; }
        protected TaskHelper TaskHelper;
        protected IConnectionService ConnectionService;
        protected ILogService LogService;

        protected BasePresenter(IConnectionService connectionService, ILogService logService)
        {
            ConnectionService = connectionService;
            LogService = logService;
            TaskHelper = new TaskHelper(connectionService, logService);
            OnDisposeCts = new CancellationTokenSource();
        }

        public void TasksCancel()
        {
            if (OnDisposeCts != null && !OnDisposeCts.IsCancellationRequested)
                OnDisposeCts.Cancel();

            OnDisposeCts = new CancellationTokenSource();
        }
    }
}

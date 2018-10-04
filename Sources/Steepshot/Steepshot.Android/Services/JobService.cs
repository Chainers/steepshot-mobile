using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Steepshot.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Jobs;
using Steepshot.Core.Sentry;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Services
{
    [Service(Label = "JobService", Enabled = true, Exported = true)]
    [IntentFilter(new[] { "Steepshot.JobService" })]
    public class JobService : Service
    {
        public const string HandlerThreadName = "F1D6C30F-1AE6-4BB6-A1B8-7E21F77AAD0E";
        public const string DataExtraName = "2E4E445F-D871-413A-BE1C-21F068C2EFDC";
        public const string DataResultExtraName = "ABC4129C-A906-4F63-9E6B-83D257FD6D55";
        public const string ActionBroadcast = "F7D31C6F-599F-4685-982E-141577B59283";

        private readonly IBinder _binder;

        private JobProcessingService _jobProcessingService;
        private Looper _serviceLooper;
        private Handler _serviceHandler;

        private bool _changingConfiguration;

        //Handler that receives messages from the thread
        //private sealed class ServiceHandler : Android.OS.Handler
        //{
        //    public ServiceHandler(Looper looper) : base(looper)
        //    {
        //    }

        //    public override void HandleMessage(Message msg)
        //    {
        //        // Normally we would do some work here, like download a file.
        //        // For our sample, we just sleep for 5 seconds.
        //        //long endTime = System.CurrentTimeMillis() + 5 * 1000;
        //        //while (System.currentTimeMillis() < endTime)
        //        //{
        //        //    synchronized(this) {
        //        //        try
        //        //        {
        //        //            wait(endTime - System.currentTimeMillis());
        //        //        }
        //        //        catch (Exception e)
        //        //        {
        //        //        }
        //        //    }
        //        //}
        //        // Stop the service using the startId, so that we don't stop
        //        // the service in the middle of handling another job

        //        //StopSelf(msg.Arg1);
        //    }
        //}

        public JobService()
        {
            _binder = new JobServiceBinder(this);
        }


        public override void OnCreate()
        {
            // Start up the thread running the service.  Note that we create a
            // separate thread because the service normally runs in the process's
            // main thread, which we don't want to block.  We also make it
            // background priority so CPU-intensive work will not disrupt our UI.
            var thread = new HandlerThread(HandlerThreadName, (int)ThreadPriority.Background);
            thread.Start();
            // Get the HandlerThread's Looper and use it for our Handler
            _serviceLooper = thread.Looper;
            _serviceHandler = new Handler(_serviceLooper);


            _jobProcessingService = AppSettings.GetJobProcessingService();
            _jobProcessingService.StartAsync();
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            _jobProcessingService.StartAsync();
            return StartCommandResult.Sticky;
        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            _changingConfiguration = true;
        }

        public override IBinder OnBind(Intent intent)
        {
            _jobProcessingService.StartAsync();

            StopForeground(true);
            _changingConfiguration = false;
            return _binder;
        }

        public override void OnRebind(Intent intent)
        {
            StopForeground(true);
            _changingConfiguration = false;
            base.OnRebind(intent);
        }

        public override bool OnUnbind(Intent intent)
        {
            //        if (!ChangingConfiguration && Utils.RequestingLocationUpdates(this))
            //        {
            //if (Build.VERSION.SdkInt ==BuildVersionCodes.O) {
            //	mNotificationManager.startServiceInForeground(new Intent(this,
            //			LocationUpdatesService.class), NOTIFICATION_ID, getNotification());
            //} else {
            //	startForeground(NOTIFICATION_ID, GetNotification());
            //}

            //            StartForeground();
            //        }
            //        return true; // Ensures onRebind() is called when a client re-binds.
            return base.OnUnbind(intent);
        }


        public override void OnDestroy()
        {
            _jobProcessingService.Stop();
        }

        public void AddJob<T>(Job job, T data) where T : SqlTableBase
        {
            _jobProcessingService.AddJob(job, data);
            _jobProcessingService.StartAsync();
        }

        public void DeleteJob(int jobId)
        {
            _jobProcessingService.DeleteJob(jobId);
        }

        public JobState GetJobState(int jobId)
        {
            return _jobProcessingService.GetJobState(jobId);
        }

        public object GetResult(int jobId)
        {
            return _jobProcessingService.GetResult(jobId);
        }
    }

    public class JobServiceBroadcastReceiver : BroadcastReceiver
    {
        public Context Context { get; set; }
        public override void OnReceive(Context context, Intent intent)
        {
            var result = intent.GetParcelableExtra(JobService.DataResultExtraName);
            if (result != null)
            {

            }
        }
    }

    public class JobServiceConnection : Object, IServiceConnection
    {
        private readonly IJobServiceContainer _container;

        public JobServiceConnection(IJobServiceContainer container)
        {
            _container = container;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = (JobServiceBinder)service;

            _container.JobService = binder.Service;
            _container.IsBound = true;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            _container.JobService = null;
            _container.IsBound = false;
        }
    }

    public class JobServiceBinder : Binder
    {
        public JobService Service { get; }

        public JobServiceBinder(JobService service)
        {
            Service = service;
        }
    }

    public interface IJobServiceContainer
    {
        bool IsBound { get; set; }

        JobService JobService { get; set; }
    }
}
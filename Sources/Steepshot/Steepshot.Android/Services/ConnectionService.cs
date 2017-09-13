using Android.App;
using Android.Net;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly ConnectivityManager connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Android.App.Activity.ConnectivityService);

        public bool IsConnectionAvailable()
        {
            var networkInfo = connectivityManager?.ActiveNetworkInfo;
            return networkInfo?.IsAvailable ?? false;
        }
    }
}

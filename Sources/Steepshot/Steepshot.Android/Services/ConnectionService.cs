using Android.App;
using Android.Net;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
    public class ConnectionService : IConnectionService
    {
        public bool IsConnectionAvailable()
        {
            ConnectivityManager connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Android.App.Activity.ConnectivityService);
            var networkInfo = connectivityManager?.ActiveNetworkInfo;
            return networkInfo?.IsAvailable ?? false;
        }
    }
}

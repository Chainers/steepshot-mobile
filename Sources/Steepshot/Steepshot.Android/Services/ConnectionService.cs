using Android.App;
using Android.Content;
using Android.Net;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
    public sealed class ConnectionService : IConnectionService
    {
        private readonly ConnectivityManager _connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);

        public bool IsConnectionAvailable()
        {
            var networkInfo = _connectivityManager?.ActiveNetworkInfo;
            return networkInfo?.IsAvailable ?? false;
        }
    }
}

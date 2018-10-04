using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Net;
using Steepshot.Base;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Utils;

namespace Steepshot.Services
{
    public sealed class ConnectionService : IConnectionService
    {
        private readonly ConnectivityManager _connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
        private bool _isConnected;
        private DateTime? _time;

        public bool IsConnectionAvailable()
        {
            //5 sec caching if connected
            if (_isConnected && _time.HasValue && (DateTime.Now - _time.Value).TotalMilliseconds < 5000)
                return _isConnected;

            var networkInfo = _connectivityManager?.ActiveNetworkInfo;
            if (networkInfo == null)
                return false;

            return networkInfo.IsAvailable && networkInfo.IsConnected && TryToConnect();
        }

        private bool TryToConnect()
        {
            if (AppSettings.ExtendedHttpClient == null)
                return false;

            try
            {
                lock (_connectivityManager)
                {
                    //5 sec caching if connected
                    if (_isConnected && _time.HasValue && (DateTime.Now - _time.Value).TotalMilliseconds < 5000)
                        return _isConnected;

                    var entry = Dns.GetHostEntry("steepshot.org");
                    _isConnected = entry.AddressList.Length > 0;
                    _time = DateTime.Now;
                }
                return _isConnected;
            }
            catch
            {
                return false;
            }
        }
    }
}

using Plugin.Connectivity;
using Steepshot.Core.Interfaces;

namespace Steepshot.iOS.Services
{
    public class ConnectionService : IConnectionService
    {
        public bool IsConnectionAvailable()
        {
            return CrossConnectivity.Current.IsConnected;
        }
    }
}


using Steepshot.Core.Clients;

namespace Steepshot.Core.Authorization
{
    public class AuthenticationManager
    {
        private SteemClient _steemClient;
        private SteepshotApiClient _steepshotSteemClient;
        private GolosClient _golosClient;
        private SteepshotApiClient _steepshotGolosClient;
        private UserManager _userManager;

        public AuthenticationManager(SteemClient steemClient, SteepshotApiClient steepshotSteemClient, GolosClient golosClient, SteepshotApiClient steepshotGolosClient, UserManager userManager)
        {
            _steemClient = steemClient;
            _steepshotSteemClient = steepshotSteemClient;
            _golosClient = golosClient;
            _steepshotGolosClient = steepshotGolosClient;
            _userManager = userManager;
        }

        public bool GetToken()
        {
            return false;
        }
    }
}

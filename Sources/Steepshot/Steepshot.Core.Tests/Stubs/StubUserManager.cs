using Steepshot.Core.Authorization;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubUserManager : UserManager
    {
        public StubUserManager(ISaverService saverService) 
            : base(saverService)
        {
        }
    }
}

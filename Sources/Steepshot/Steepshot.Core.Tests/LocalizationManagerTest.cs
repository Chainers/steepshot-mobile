using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class LocalizationManagerTest : BaseTests
    {
        [Test]
        public void Test()
        {
            var acc = AppSettings.LocalizationManager.GetText(LocalizationKeys.Account);
            Assert.IsFalse(string.IsNullOrEmpty(acc));
        }
    }
}

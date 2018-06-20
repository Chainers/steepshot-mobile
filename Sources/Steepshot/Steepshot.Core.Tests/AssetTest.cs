using Newtonsoft.Json;
using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using System.Collections.Generic;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class AssetTest : BaseTests
    {
        [Test]
        public void LocalizationManagerTest()
        {
            var acc = AppSettings.LocalizationManager.GetText(LocalizationKeys.Account);
            Assert.IsFalse(string.IsNullOrEmpty(acc));
        }

        [Test]
        public void IntegrationModuleConfigTest()
        {
            var result = AppSettings.AssetHelper.IntegrationModuleConfig();
            Assert.IsTrue(result.Count > 0);
        }
    }
}

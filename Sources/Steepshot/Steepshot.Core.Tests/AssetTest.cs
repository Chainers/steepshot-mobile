using NUnit.Framework;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class AssetTest : BaseTests
    {
        [Test]
        public void LocalizationManagerTest()
        {
            var lm = Container.GetLocalizationManager();
            var acc = lm.GetText(LocalizationKeys.Account);
            Assert.IsFalse(string.IsNullOrEmpty(acc));
        }

        [Test]
        public void IntegrationModuleConfigTest()
        {
            var assetHelper = Container.GetAssetHelper();
            var result = assetHelper.IntegrationModuleConfig();
            Assert.IsTrue(result.Count > 0);
        }
    }
}

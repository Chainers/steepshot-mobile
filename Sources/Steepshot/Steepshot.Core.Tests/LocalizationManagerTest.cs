using System;
using System.IO;
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
            var en = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\dic.xml");
            var lm = AppSettings.LocalizationManager;
            lm.Model.Version = 0;
            Assert.IsTrue(lm.Update(en));
            var acc = lm.GetText(LocalizationKeys.Account);
            Assert.IsFalse(string.IsNullOrEmpty(acc));
        }
    }
}

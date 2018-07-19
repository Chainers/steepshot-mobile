using System;
using System.IO;
using NUnit.Framework;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class LocalizationManagerTest : BaseTests
    {
        [Test]
        public void Test()
        {
            var en = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\dic.xml");
            var lm = new LocalizationManager(new LocalizationModel());
            Assert.IsTrue(lm.Reset(en));
            var acc = lm.GetText(LocalizationKeys.Account);
            Assert.IsFalse(string.IsNullOrEmpty(acc));
        }
    }
}

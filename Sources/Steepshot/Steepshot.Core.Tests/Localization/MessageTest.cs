using System;
using NUnit.Framework;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests.Localization
{
    [TestFixture]
    public class MessageTest : BaseTests
    {
        public LocalizationManager LocalizationManager => AppSettings.LocalizationManager;

        [Test]
        [TestCase("<h1>Server Error (500)</h1>")]
        [TestCase("Wrong identifier")]
        [TestCase("The submitted file is empty")]
        [TestCase("13 N5boost16exception_detail10clone_implINS0_19error_info_injectorISt12out_of_rangeEEEE: unknown key")]
        [TestCase("Category used for offset was not found")]
        [TestCase("4100000 plugin_exception: plugin exception: The comment is archived")]
        [TestCase("4100000 plugin_exception: plugin exception Account: steem bandwidth limit exceeded. Please wait to transact or power up STEEM")]
        [TestCase("4100000 plugin_exception: plugin exception: Account: ${account} bandwidth limit exceeded. Please wait to transact or power up STEEM")]
        [TestCase("3030000 tx_missing_posting_auth: missing required posting authority")]
        [TestCase("Size of the uploaded file is too big. Max size: 10 MB")]
        [TestCase("10 assert_exception: Assert Exception: itr->vote_percent != o.weight: You have already voted in a similar way")]
        [TestCase(@"<html><head><title>413 Request Entity Too Large</title></head>")]
        public void GetText(string key)
        {
            Console.Write($"{key} => ");
            Assert.IsTrue(LocalizationManager.ContainsKey(key));
            var text = LocalizationManager.GetText(key);
            Console.WriteLine(text);
            Assert.IsFalse(string.IsNullOrEmpty(text));
        }

        [Test]
        public void LocalizationKeysTest()
        {
            Assert.IsTrue(LocalizationManager.Model.Version == 15);

            var str = string.Empty;
            var keys = Enum.GetNames(typeof(LocalizationKeys));

            foreach (var key in keys)
            {
                if (!LocalizationManager.ContainsKey(key))
                    str += " {key}";
            }

            Assert.IsTrue(string.IsNullOrEmpty(str), str);
        }

        [Test]
        public void LocalizationVersionTest()
        {
            Assert.IsTrue(LocalizationManager.Model.Version == 15);
        }
    }
}

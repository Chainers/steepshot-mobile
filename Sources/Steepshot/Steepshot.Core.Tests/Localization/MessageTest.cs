using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
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
            Assert.IsFalse(string.IsNullOrEmpty(LocalizationManager.GetText(key)));
            var text = LocalizationManager.GetText(key);
            Console.WriteLine(text);
            Assert.IsFalse(string.IsNullOrEmpty(text));
        }

        [Test]
        public void LocalizationKeysTest()
        {
            Assert.AreEqual(LocalizationManager.Model.Version, 16);

            var str = string.Empty;
            var keys = Enum.GetNames(typeof(LocalizationKeys));

            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty(LocalizationManager.GetText(key)))
                    str += $" {key}";
            }

            Assert.IsTrue(string.IsNullOrEmpty(str), str);
        }

        [Test]
        public void LocalizationVersionTest()
        {
            Assert.IsTrue(LocalizationManager.Model.Version == 15);
        }

        [Test]
        public void JsonCompereTest()
        {
            var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\Localization.en-us.txt");
            var lm = JsonConvert.DeserializeObject<LocalizationModel>(json);

            var ls = new List<string>();

            foreach (var itm in lm.Map)
            {
                var t = LocalizationManager.GetText(itm.Key);

                if (!t.Equals(itm.Value))
                {
                    ls.Add($"{itm.Key} |> {itm.Value}");
                }
            }
            Console.WriteLine(string.Join(Environment.NewLine, ls));
            Assert.IsFalse(ls.Any());
        }

        [Test]
        public void XmlCompereTest()
        {
            var json = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Data\\dicOld.xml");
            var lm = new LocalizationModel();
            LocalizationManager.Update(json, lm);

            var ls = new List<string>();

            foreach (var itm in lm.Map)
            {
                var t = LocalizationManager.GetText(itm.Key);

                if (!t.Equals(itm.Value))
                {
                    ls.Add($"{itm.Key} |> {itm.Value}");
                }
            }
            Console.WriteLine(string.Join(Environment.NewLine, ls));
            Assert.IsFalse(ls.Any());
        }

    }
}

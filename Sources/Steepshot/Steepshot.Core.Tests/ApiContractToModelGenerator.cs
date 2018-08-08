using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Ditch.EOS;
using NUnit.Framework;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class ApiContractToModelGenerator
    {
        protected static OperationManager Api;

        [OneTimeSetUp]
        protected virtual void OneTimeSetUp()
        {
            if (Api == null)
            {
                Api = new OperationManager
                {
                    ChainUrl = ConfigurationManager.AppSettings["ChainUrl"],
                    WalletUrl = ConfigurationManager.AppSettings["WalletUrl"]
                };
            }
        }

        protected string GetTestImagePath()
        {
            var currentDir = AppContext.BaseDirectory;
            var parent = Directory.GetParent(currentDir).Parent;
            return Path.Combine(parent.FullName, @"Data/cat.jpg");
        }


        [Test]
        [TestCase("eosio", @"..\..\..\Steepshot.Core\Models\Contracts\", new[] { "claimrewards" })]
        [TestCase("eosio.token", @"..\..\..\Steepshot.Core\Models\Contracts\", new[] { "transfer" })]
        //[TestCase("vimproxy", @"..\..\..\Steepshot.Core\Models\Contracts\", null)]
        //[TestCase("vimmedia", @"..\..\..\Steepshot.Core\Models\Contracts\", new[] { "createpost" })]
        [TestCase("vimtoken", @"..\..\..\Steepshot.Core\Models\Contracts\", new[] { "powerdown", "powerup", "transfer" })]
        public async Task Generate(string contractName, string outDir, string[] set)
        {
            HashSet<string> hs = null;
            if (set != null)
                hs = new HashSet<string>(set);
            
            var currentDir = AppContext.BaseDirectory;
            outDir = $"{currentDir}{outDir}";

            var generator = new ContractCodeGenerator();
            await generator.Generate(Api, contractName, "Steepshot.Core.Models.Contracts", outDir, hs, CancellationToken.None);
        }

    }
}

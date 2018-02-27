using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Autofac;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Services;
using Steepshot.Core.Tests.Stubs;
using Steepshot.Core.Utils;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        private const bool IsDev = true;
        protected static readonly Dictionary<KnownChains, UserInfo> Users;
        protected static readonly Dictionary<KnownChains, SteepshotApiClient> Api;

        static BaseTests()
        {
            var builder = new ContainerBuilder();
            
            var jsonLocalization = File.ReadAllText($"{AppDomain.CurrentDomain.BaseDirectory}\\Localization.en-us.txt");
            builder.RegisterInstance(new StubAppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterInstance(new StubDataProvider()).As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new StubSaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new StubConnectionService()).As<IConnectionService>().SingleInstance();
            builder.RegisterInstance(new LocalizationManager(JsonConvert.DeserializeObject<LocalizationModel>(jsonLocalization))).As<LocalizationManager>().SingleInstance();
            builder.RegisterType<StubReporterService>().As<IReporterService>().SingleInstance();

            AppSettings.Container = builder.Build();
            AppSettings.IsDev = IsDev;

            Users = new Dictionary<KnownChains, UserInfo>
            {
                {KnownChains.Steem, new UserInfo {Login = ConfigurationManager.AppSettings["SteemLogin"], PostingKey = ConfigurationManager.AppSettings["SteemPostingWif"]}},
                {KnownChains.Golos, new UserInfo {Login = ConfigurationManager.AppSettings["GolosLogin"], PostingKey = ConfigurationManager.AppSettings["GolosPostingWif"]}},
            };

            Api = new Dictionary<KnownChains, SteepshotApiClient>
            {
                {KnownChains.Steem, new SteepshotApiClient()},
                {KnownChains.Golos, new SteepshotApiClient()},
            };

            Api[KnownChains.Steem].InitConnector(KnownChains.Steem, IsDev);
            Api[KnownChains.Golos].InitConnector(KnownChains.Golos, IsDev);
        }

        protected string GetTestImagePath()
        {
            var currentDir = AppContext.BaseDirectory;
            var parent = Directory.GetParent(currentDir).Parent;
            return Path.Combine(parent.FullName, @"Data/cat.jpg");
        }

        protected void AssertResult<T>(OperationResult<T> response, bool throwIfError = true)
        {
            Assert.NotNull(response, "Response is null");

            if (response.IsSuccess)
            {
                Assert.NotNull(response.Result, "Response is success, but result is NULL");
                Console.WriteLine(JsonConvert.SerializeObject(response.Result));
                Assert.IsNull(response.Error, "Response is success, but errors array is NOT empty");
            }
            else
            {
                Assert.IsNull(response.Result, "Response is failed, but result is NOT null");
                Assert.IsNotNull(response.Error, "Response is failed, but errors array is EMPTY");

                Console.WriteLine(response.Error.Message);
                if (throwIfError)
                    Assert.IsTrue(response.IsSuccess);
            }
        }

        public List<ValidationResult> Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            return results;
        }
    }
}
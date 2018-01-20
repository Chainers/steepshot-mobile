﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading;
using Autofac;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Services;
using Steepshot.Core.Tests.Stubs;
using Steepshot.Core.Utils;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        private const bool IsDev = false;
        protected static readonly Dictionary<KnownChains, UserInfo> Users;
        protected static readonly Dictionary<KnownChains, SteepshotApiClient> Api;

        static BaseTests()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new StubAppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterInstance(new StubDataProvider()).As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new StubSaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new StubConnectionService()).As<IConnectionService>().SingleInstance();
            builder.RegisterType<StubReporterService>().As<IReporterService>().SingleInstance();


            AppSettings.Container = builder.Build();
            AppSettings.IsDev = IsDev;

            Users = new Dictionary<KnownChains, UserInfo>
            {
                {KnownChains.Steem, new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["SteemWif"]}},
                {KnownChains.Golos, new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["GolosWif"]}},
            };

            Api = new Dictionary<KnownChains, SteepshotApiClient>
            {
                {KnownChains.Steem, new SteepshotApiClient()},
                {KnownChains.Golos, new SteepshotApiClient()},
            };

            Api[KnownChains.Steem].InitConnector(KnownChains.Steem, IsDev, CancellationToken.None);
            Api[KnownChains.Golos].InitConnector(KnownChains.Golos, IsDev, CancellationToken.None);
        }

        protected string GetTestImagePath()
        {
            var currentDir = AppContext.BaseDirectory;
            var parent = Directory.GetParent(currentDir).Parent;
            return Path.Combine(parent.FullName, @"Data/cat.jpg");
        }

        protected void AssertResult<T>(OperationResult<T> response)
        {
            Assert.NotNull(response, "Response is null");

            if (response.IsSuccess)
            {
                Assert.NotNull(response.Result, "Response is success, but result is NULL");
                Assert.IsNull(response.Error, "Response is success, but errors array is NOT empty");
            }
            else
            {
                Assert.IsNull(response.Result, "Response is failed, but result is NOT null");
                Assert.IsNotNull(response.Error, "Response is failed, but errors array is EMPTY");

                Console.WriteLine(response.Error.Message);
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
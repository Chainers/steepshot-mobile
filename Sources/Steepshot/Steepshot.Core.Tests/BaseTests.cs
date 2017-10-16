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

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        private const bool IsDev = false;
        protected static readonly Dictionary<string, UserInfo> Users;
        protected static readonly Dictionary<string, ISteepshotApiClient> Api;

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
            Users = new Dictionary<string, UserInfo>
            {
                {"Steem", new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["SteemWif"]}},
                {"Golos", new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["GolosWif"]}}
            };

            Api = new Dictionary<string, ISteepshotApiClient>
            {
                {"Steem", new DitchApi()},
                {"Golos", new DitchApi()}
            };

            Api["Steem"].Connect(KnownChains.Steem, IsDev);
            Api["Golos"].Connect(KnownChains.Golos, IsDev);
        }

        protected UserInfo Authenticate(string name)
        {
            //ISteepshotApiClient api = Api[name];
            UserInfo user = Users[name];

            //// Arrange
            //var request = new AuthorizedRequest(user);

            //// Act
            //var response = api.LoginWithPostingKey(request).Result;

            //// Assert
            //AssertResult(response);
            //Assert.That(response.Result.IsLoggedIn, Is.True);
            //user.SessionId = response.Result.SessionId;
            return user;
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

            if (response.Success)
            {
                Assert.NotNull(response.Result, "Response is success, but result is NULL");
                Assert.IsEmpty(response.Errors, "Response is success, but errors array is NOT empty");
            }
            else
            {
                Assert.IsNull(response.Result, "Response is failed, but result is NOT null");
                Assert.IsNotEmpty(response.Errors, "Response is failed, but errors array is EMPTY");

                foreach (var error in response.Errors)
                {
                    Console.WriteLine(error);
                }
            }
        }
    }
}
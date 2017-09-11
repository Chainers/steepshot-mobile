using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Ditch;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Services;
using Steepshot.Core.Tests.Stubs;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        protected static readonly Dictionary<string, UserInfo> Users;
        protected static readonly Dictionary<string, ISteepshotApiClient> Api;
        protected static readonly Dictionary<string, ChainInfo> Chain;

        static BaseTests()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(new StubAppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterInstance(new StubDataProvider()).As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(new StubSaverService()).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new StubConnectionService()).As<IConnectionService>().SingleInstance();
            builder.RegisterType<StubReporterService>().As<IReporterService>().SingleInstance();


            AppSettings.Container = builder.Build();

            Users = new Dictionary<string, UserInfo>()
            {
                {"Steem",new UserInfo{Login = "joseph.kalu", PostingKey = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG"}},
                {"Golos",new UserInfo{Login = "joseph.kalu", PostingKey = "5JXCxj6YyyGUTJo9434ZrQ5gfxk59rE3yukN42WBA6t58yTPRTG"}}
            };

            Api = new Dictionary<string, ISteepshotApiClient>
            {
                //{"Steem", new SteepshotApiClient(Constants.SteemUrl)},
                //{"Golos", new SteepshotApiClient(Constants.GolosUrl)}
                
                {"Steem", new DitchApi(KnownChains.Steem, false)},
                {"Golos", new DitchApi(KnownChains.Golos, false)}
            };
        }

        protected UserInfo Authenticate(string name)
        {
            ISteepshotApiClient api = Api[name];
            UserInfo user = Users[name];

            // Arrange
            var request = new AuthorizedRequest(user);

            // Act
            var response = api.LoginWithPostingKey(request).Result;

            // Assert
            AssertResult(response);
            Assert.That(response.Result.IsLoggedIn, Is.True);
            user.SessionId = response.Result.SessionId;
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
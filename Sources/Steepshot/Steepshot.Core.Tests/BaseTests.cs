using System;
using System.Collections.Generic;
using System.IO;
using Ditch;
using NUnit.Framework;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        protected static readonly Dictionary<string, UserInfo> Users;
        protected static readonly Dictionary<string, ISteepshotApiClient> Api;
        protected static readonly Dictionary<string, ChainInfo> Chain;

        static BaseTests()
        {
            Users = new Dictionary<string, UserInfo>()
            {
                {"Steem",new UserInfo{Login = "joseph.kalu", PostingKey = "***REMOVED***"}},
                {"Golos",new UserInfo{Login = "joseph.kalu", PostingKey = "***REMOVED***"}}
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
            Assert.That("User was logged in.", Is.EqualTo(response.Result.Message));
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
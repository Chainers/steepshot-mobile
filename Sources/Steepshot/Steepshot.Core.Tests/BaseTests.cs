using System;
using System.IO;
using NUnit.Framework;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        private readonly ISteepshotApiClient _apiSteem = new SteepshotApiClient(Constants.SteemUrl);
        private readonly ISteepshotApiClient _apiGolos = new SteepshotApiClient(Constants.GolosUrl);

        protected ISteepshotApiClient Api(string name)
        {
            switch (name)
            {
                case "Steem":
                    return _apiSteem;
                case "Golos":
                    return _apiGolos;
                default:
                    return null;
            }
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
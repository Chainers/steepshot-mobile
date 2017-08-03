using System;
using System.Configuration;
using NUnit.Framework;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        protected readonly SteepshotApiClient _steem = new SteepshotApiClient(ConfigurationManager.AppSettings["steem_url"]);
        protected readonly SteepshotApiClient _golos = new SteepshotApiClient(ConfigurationManager.AppSettings["golos_url"]);

        protected SteepshotApiClient Api(string name)
        {
            switch (name)
            {
                case "Steem":
                    return _steem;
                case "Golos":
                    return _golos;
                default:
                    return null;
            }
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
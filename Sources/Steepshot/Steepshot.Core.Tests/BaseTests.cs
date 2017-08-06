using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        protected static IConfigurationRoot Configuration { get; set; }
        protected readonly ISteepshotApiClient _steem = new SteepshotApiClient(Configuration["steem_url"]);
        protected readonly ISteepshotApiClient _golos = new SteepshotApiClient(Configuration["golos_url"]);

        protected BaseTests()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        protected ISteepshotApiClient Api(string name)
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

        protected string TestImagePath()
        {
            var currentDir = Directory.GetCurrentDirectory();
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
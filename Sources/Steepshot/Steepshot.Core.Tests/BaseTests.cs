using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        protected static IContainer Container;
        protected static IAppInfo AppInfo;
        protected static LocalizationManager LocalizationManager;

        protected static readonly Dictionary<KnownChains, User> Users;
        protected static readonly Dictionary<KnownChains, BaseDitchClient> Api;
        protected static readonly Dictionary<KnownChains, SteepshotApiClient> SteepshotApi;
        protected static readonly SteepshotClient SteepshotClient;

        static BaseTests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<IocModule>();
            Container = builder.Build();

            SteepshotClient = Container.GetSteepshotClient();
            AppInfo = Container.GetAppInfo();
            LocalizationManager = Container.GetLocalizationManager();

            Api = new Dictionary<KnownChains, BaseDitchClient>
            {
                {KnownChains.Steem, Container.GetDitchClient(KnownChains.Steem)},
                {KnownChains.Golos, Container.GetDitchClient(KnownChains.Golos)}
            };

            SteepshotApi = new Dictionary<KnownChains, SteepshotApiClient>
            {
                {KnownChains.Steem, Container.GetSteepshotApiClient(KnownChains.Steem)},
                {KnownChains.Golos, Container.GetSteepshotApiClient(KnownChains.Golos)}
            };

            Users = new Dictionary<KnownChains, User>
            {
                {KnownChains.Steem, Container.GetUser()},
                {KnownChains.Golos, Container.GetUser()}
            };

            Users[KnownChains.Steem].SwitchUser(new UserInfo { Login = ConfigurationManager.AppSettings["SteemLogin"], Chain = KnownChains.Steem });
            Users[KnownChains.Golos].SwitchUser(new UserInfo { Login = ConfigurationManager.AppSettings["GolosLogin"], Chain = KnownChains.Golos });
        }

        private void InitIoC()
        {
            if (Container == null)
            {
                var builder = new ContainerBuilder();

                builder.RegisterModule<IocModule>();

                Container = builder.Build();
            }
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
                Assert.IsNull(response.Exception, "Response is success, but errors array is NOT empty");
            }
            else
            {
                Assert.IsNull(response.Result, "Response is failed, but result is NOT null");
                Assert.IsNotNull(response.Exception, "Response is failed, but errors array is EMPTY");

                Console.WriteLine(response.Exception.Message);
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

        protected async Task<OperationResult<VoidResponse>> CreateOrEditCommentAsync(KnownChains chains, CreateOrEditCommentModel model, CancellationToken ct)
        {
            if (!model.IsEditMode)
                model.Beneficiaries = await SteepshotApi[chains].GetBeneficiariesAsync(ct).ConfigureAwait(false);

            var result = await Api[chains].CreateOrEditAsync(model, ct).ConfigureAwait(false);
            //log parent post to perform update
            await SteepshotApi[chains].TraceAsync($"post/@{model.ParentAuthor}/{model.ParentPermlink}/comment", model.Login, result.Exception, $"@{model.ParentAuthor}/{model.ParentPermlink}", ct).ConfigureAwait(false);
            return result;
        }
    }
}
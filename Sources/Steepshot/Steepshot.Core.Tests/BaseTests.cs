using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Autofac;
using NUnit.Framework;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Tests.Stubs;
using Steepshot.Core.Utils;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Tests
{
    public class BaseTests
    {
        private const bool IsDev = true;
        protected static readonly Dictionary<KnownChains, User> Users;
        protected static readonly Dictionary<KnownChains, BaseDitchClient> Api;
        protected static readonly Dictionary<KnownChains, SteepshotApiClient> SteepshotApi;
        protected static readonly SteepshotClient steepshotClient;

        static BaseTests()
        {
            var builder = new ContainerBuilder();

            var saverService = new StubSaverService();
            var logService = new StubLogService();
            var assetsHelper = new AssetsHelperStub();
            var configManager = new ConfigManager(saverService, assetsHelper);
            var connectionService = new StubConnectionService();
            var extendedHttpClient = new ExtendedHttpClient();

            var lm = new LocalizationManager(saverService, assetsHelper, connectionService, logService);

            builder.RegisterInstance(assetsHelper).As<IAssetHelper>().SingleInstance();
            builder.RegisterInstance(new StubAppInfo()).As<IAppInfo>().SingleInstance();
            builder.RegisterInstance(new UserManager(saverService)).As<UserManager>().SingleInstance();
            builder.RegisterInstance(saverService).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(new StubConnectionService()).As<IConnectionService>().SingleInstance();
            builder.RegisterInstance(lm).As<LocalizationManager>().SingleInstance();
            builder.RegisterType<StubLogService>().As<ILogService>().SingleInstance();

            AppSettings.Container = builder.Build();
            AppSettings.Settings.IsDev = IsDev;

            // = new UserInfo {Login = ConfigurationManager.AppSettings["SteemLogin"], PostingKey = ConfigurationManager.AppSettings["SteemPostingWif"]}},
            //{Login = ConfigurationManager.AppSettings["GolosLogin"], PostingKey = ConfigurationManager.AppSettings["GolosPostingWif"]}},
            Users = new Dictionary<KnownChains, User>
            {
                //{KnownChains.Steem, new User(new StubUserManager(new)),
                //{KnownChains.Golos, new User(new StubUserManager()) ,
            };

            Api = new Dictionary<KnownChains, BaseDitchClient>
            {
                {KnownChains.Steem, new SteemClient(extendedHttpClient, logService, configManager)},
                {KnownChains.Golos, new GolosClient(extendedHttpClient, logService, configManager)},
            };

            SteepshotApi = new Dictionary<KnownChains, SteepshotApiClient>
            {
                {KnownChains.Steem, new SteepshotApiClient(extendedHttpClient, logService, Constants.SteemUrl)},
                {KnownChains.Golos, new SteepshotApiClient(extendedHttpClient, logService, Constants.GolosUrl)},
            };
            steepshotClient = new SteepshotClient(extendedHttpClient);
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
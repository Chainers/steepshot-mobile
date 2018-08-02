using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Services;

namespace Steepshot.Core.Clients
{
    public class ConfigManager
    {
        public const string SteemUpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/Sources/Steepshot/Steepshot.Android/Assets/SteemNodesConfig.txt";
        public const string GolosUpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/Sources/Steepshot/Steepshot.Android/Assets/GolosNodesConfig.txt";
        private const string SteemNodeConfigKey = "SteemNodeConfigKey";
        private const string GolosNodeConfigKey = "GolosNodeConfigKey";
        private readonly ISaverService _saverService;

        public double SteemPerVestsRatio { get; set; }
        public double GolosPerGestsRatio { get; set; }

        public List<NodeConfig> SteemNodeConfigs { get; private set; }
        public List<NodeConfig> GolosNodeConfigs { get; private set; }

        public ConfigManager(ISaverService saverService, IAssetHelper assetHelper)
        {
            _saverService = saverService;

            SteemNodeConfigs = _saverService.Get<List<NodeConfig>>(SteemNodeConfigKey);
            if (SteemNodeConfigs == null || !SteemNodeConfigs.Any())
                SteemNodeConfigs = assetHelper.SteemNodesConfig();

            GolosNodeConfigs = _saverService.Get<List<NodeConfig>>(GolosNodeConfigKey);
            if (GolosNodeConfigs == null || !GolosNodeConfigs.Any())
                GolosNodeConfigs = assetHelper.GolosNodesConfig();
        }

        public async void UpdateGlobalFundRatio(SteepshotApiClient steepshotApiClient)
        {
            var properties = await steepshotApiClient.GetDynamicGlobalProperties(CancellationToken.None);
            switch (steepshotApiClient.Chain)
            {
                case KnownChains.Golos:
                    {
                        if (properties.IsSuccess)
                        {
                            GolosPerGestsRatio = properties.Result.TotalVestingFund / properties.Result.TotalVestingShares;
                        }
                        break;
                    }
                case KnownChains.Steem:
                    {
                        if (properties.IsSuccess)
                        {
                            SteemPerVestsRatio = properties.Result.TotalVestingFund / properties.Result.TotalVestingShares;
                        }
                        break;
                    }
            }
        }

        public async Task Update(SteepshotApiClient steepshotApiClient, CancellationToken token)
        {
            switch (steepshotApiClient.Chain)
            {
                case KnownChains.Golos:
                    {
                        var conf = await steepshotApiClient.HttpClient.Get<List<NodeConfig>>(GolosUpdateUrl, token);
                        if (conf.IsSuccess)
                        {
                            GolosNodeConfigs = conf.Result;
                            _saverService.Save(GolosNodeConfigKey, GolosNodeConfigs);
                        }
                        break;
                    }
                case KnownChains.Steem:
                    {
                        var conf = await steepshotApiClient.HttpClient.Get<List<NodeConfig>>(SteemUpdateUrl, token);
                        if (conf.IsSuccess)
                        {
                            SteemNodeConfigs = conf.Result;
                            _saverService.Save(SteemNodeConfigKey, SteemNodeConfigs);
                        }
                        break;
                    }
            }
        }
    }
}

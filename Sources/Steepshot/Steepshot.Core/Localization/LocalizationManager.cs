using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        public const string UpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/References/Languages/{0}/dic.xml";
        public const string Localization = "Localization";
        public const string DefaultLang = "en";

        private static readonly string[] Separator = { "&split;" };

        private readonly ILogService _logService;
        private readonly IConnectionService _connectionService;
        private readonly ISaverService _saverService;
        private readonly ExtendedHttpClient _httpClient;
        private readonly Dictionary<string, LocalizationModel> _localizationModels;

        public LocalizationModel Model { get; }


        public LocalizationManager(ISaverService saverService, IAssetHelper assetHelper, IConnectionService connectionService, ILogService logService, ExtendedHttpClient httpClient)
        {
            _saverService = saverService;
            _connectionService = connectionService;
            _logService = logService;
            _httpClient = httpClient;
            _localizationModels = _saverService.Get<Dictionary<string, LocalizationModel>>(Localization) ?? new Dictionary<string, LocalizationModel>();

            if (_localizationModels.ContainsKey(DefaultLang))
            {
                Model = _localizationModels[DefaultLang];
            }
            else
            {
                var txt = assetHelper.GetLocalization(DefaultLang);
                Model = new LocalizationModel();
                Update(txt, Model);
            }
        }


        public LocalizationModel SelectLocalization(string lang)
        {
            if (_localizationModels.ContainsKey(lang))
                return _localizationModels[lang];
            return null;
        }

        public async Task UpdateAsync(CancellationToken token)
        {
            var available = _connectionService.IsConnectionAvailable();
            if (!available)
                return;

            var rez = await _httpClient.GetAsync<string>(string.Format(UpdateUrl, Model.Lang), token)
                .ConfigureAwait(false);
            if (!rez.IsSuccess)
                return;

            var xml = rez.Result;
            var changed = Update(xml, Model);
            if (changed)
            {
                if (!_localizationModels.ContainsKey(Model.Lang))
                    _localizationModels.Add(Model.Lang, Model);
                _saverService.Save(Localization, _localizationModels);
            }
        }

        public bool Update(string xml, LocalizationModel model)
        {
            XmlTextReader reader = null;
            StringReader sReader = null;
            try
            {
                sReader = new StringReader(xml);
                reader = new XmlTextReader(sReader);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("resources") && reader.AttributeCount == 1)
                    {
                        var version = reader.GetAttribute("version");
                        if (version == null || int.Parse(version) <= model.Version)
                            return false;

                        model.Version = int.Parse(version);
                        break;
                    }
                }

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("string") && reader.HasAttributes)
                    {
                        var json = reader.GetAttribute("name");
                        if (json == null)
                            continue;

                        var names = json.StartsWith("[")
                            ? JsonConvert.DeserializeObject<string[]>(json)
                            : json.Split(Separator, StringSplitOptions.None);

                        reader.Read();
                        var value = reader.Value;

                        foreach (var name in names)
                        {
                            value = value.Replace("\\\"", "\"");
                            if (model.Map.ContainsKey(name))
                                model.Map[name] = value;
                            else
                                model.Map.Add(name, value);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logService.WarningAsync(ex);
            }
            finally
            {
                sReader?.Close();
                reader?.Close();
            }
            return false;
        }

        public string GetText(ValidationException validationException)
        {
            if (validationException.Key.HasValue)
            {
                return GetText(validationException.Key.ToString(), validationException.Parameters);
            }
            return GetText(validationException.Message);
        }

        public string GetText(LocalizationKeys key, params object[] args)
        {
            var ks = key.ToString();
            return GetText(ks, args);
        }

        public static string NormalizeKey(string key)
        {
            return key.Replace('\r', ' ').Replace('\n', ' ').Replace("  ", " ");
        }

        public string GetText(string key, params object[] args)
        {
            var result = string.Empty;

            if (Model.Map.ContainsKey(key))
            {
                if (args != null && args.Length > 0)
                    result = string.Format(Model.Map[key], args);
                else
                    result = Model.Map[key];
            }
            else
            {
                key = NormalizeKey(key);
                foreach (var item in Model.Map)
                {
                    if (key.StartsWith(item.Key))
                    {
                        result = item.Value;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(result))
                {
                    var keyLength = 0;
                    foreach (var item in Model.Map)
                    {
                        if (key.Contains(item.Key) && keyLength < item.Key.Length)
                        {
                            result = item.Value;
                            keyLength = item.Key.Length;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                var ex = new Exception($"Key not found: {key}");
                _logService.InfoAsync(ex);
            }

            return result;
        }
    }
}

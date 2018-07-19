using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Services;

namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        public const string UpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/References/Languages/{0}/dic.xml";
        public const string Localization = "Localization";
        public const string DefaultLang = "en";

        private readonly ISaverService _saverService;
        private readonly Dictionary<string, LocalizationModel> _localizationModel;

        public LocalizationModel Model { get; }


        public LocalizationManager(ISaverService saverService, IAssetsHelper assetsHelper)
        {
            _saverService = saverService;
            _localizationModel = _saverService.Get<Dictionary<string, LocalizationModel>>(Localization);

            Model = _localizationModel.ContainsKey(DefaultLang)
                ? _localizationModel[DefaultLang]
                : assetsHelper.GetLocalization(DefaultLang);
        }


        public LocalizationModel SelectLocalization(string lang)
        {
            if (_localizationModel.ContainsKey(lang))
                return _localizationModel[lang];
            return null;
        }

        public async void Update(ApiGateway gateway)
        {
            var rez = await gateway.Get<string>(string.Format(UpdateUrl, Model.Lang), CancellationToken.None);
            if (!rez.IsSuccess)
                return;

            var xml = rez.Result;
            var changed = Update(xml);
            if (changed)
                _saverService.Save(Localization, _localizationModel);
        }

        public bool Update(string xml)
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
                        if (version == null || int.Parse(version) <= Model.Version)
                            return false;

                        Model.Version = int.Parse(version);
                        break;
                    }
                }

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("string") && reader.HasAttributes)
                    {
                        var json = reader.GetAttribute("name");
                        var names = JsonConvert.DeserializeObject<string[]>(json);
                        reader.Read();
                        var value = reader.Value;

                        foreach (var name in names)
                        {
                            if (Model.Map.ContainsKey(name))
                                Model.Map[name] = value;
                            else
                                Model.Map.Add(name, value);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                //TODO log Warn
            }
            finally
            {
                sReader?.Close();
                reader?.Close();
            }
            return false;
        }

        public string GetText(LocalizationKeys key, params object[] args)
        {
            var ks = key.ToString();
            return GetText(ks, args);
        }

        public bool ContainsKey(string key)
        {
            var contains = Model.Map.ContainsKey(key);
            if (!contains)
            {
                key = NormalizeKey(key);
                foreach (var item in Model.Map)
                {
                    if (key.StartsWith(item.Key))
                        return true;
                }
            }
            return contains;
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
                    foreach (var item in Model.Map)
                    {
                        if (key.Contains(item.Key))
                        {
                            result = item.Value;
                            break;
                        }
                    }
                }
            }
            return result;
        }
    }
}

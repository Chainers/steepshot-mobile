using System.Collections.Generic;
using System.Threading;
using Steepshot.Core.Clients;
using Steepshot.Core.Errors;
using Steepshot.Core.Services;

namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        public const string Localization = "Localization";
        private const string UpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/Sources/Steepshot/Steepshot.Android/Assets/Localization.en-us.txt";
        public const string DefaultLang = "en-us";

        private readonly ISaverService _saverService;
        private readonly Dictionary<string, LocalizationModel> _localizationModel;

        public LocalizationModel Model { get; }


        public LocalizationManager(ISaverService saverService, IAssetHelper assetHelper)
        {
            _saverService = saverService;
            _localizationModel = _saverService.Get<Dictionary<string, LocalizationModel>>(Localization);

            Model = _localizationModel.ContainsKey(DefaultLang)
                ? _localizationModel[DefaultLang]
                : assetHelper.GetLocalization(DefaultLang);
        }


        public LocalizationModel SelectLocalization(string lang)
        {
            if (_localizationModel.ContainsKey(lang))
                return _localizationModel[lang];
            return null;
        }

        public async void Update(ExtendedHttpClient gateway)
        {
            var rez = await gateway.Get<LocalizationModel>(UpdateUrl, CancellationToken.None);
            if (!rez.IsSuccess)
                return;

            var model = rez.Result;
            var changed = Reset(model);
            if (changed)
            {
                if (_localizationModel.ContainsKey(model.Lang))
                    _localizationModel[model.Lang] = model;
                else
                    _localizationModel.Add(model.Lang, model);

                _saverService.Save(Localization, _localizationModel);
            }
        }

        private bool Reset(LocalizationModel model)
        {
            try
            {
                var changed = false;
                if (Model.Lang.Equals(model.Lang) && model.Version > Model.Version)
                {
                    changed = true;
                    foreach (var item in model.Map)
                    {
                        if (Model.Map.ContainsKey(item.Key))
                        {
                            Model.Map[item.Key] = item.Value;
                        }
                        else
                        {
                            Model.Map.Add(item.Key, item.Value);
                        }
                    }
                    Model.Version = model.Version;
                }
                return changed;
            }
            catch
            {
                //to do nothing
            }
            return false;
        }

        public string GetText(ValidationError validationError)
        {
            if (validationError.Key.HasValue)
            {
                return GetText(validationError.Key.ToString(), validationError.Parameters);
            }
            return GetText(validationError.Message);
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

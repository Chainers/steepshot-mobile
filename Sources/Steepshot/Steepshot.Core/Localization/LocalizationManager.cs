using Newtonsoft.Json;

namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        public const string UpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/message-wrapper/Sources/Steepshot/Steepshot.Android/Assets/Localization.en-us.txt";

        public LocalizationModel Model { get; }

        public LocalizationManager(LocalizationModel model)
        {
            Model = model;
        }

        public bool Reset(string content)
        {
            try
            {
                var changed = false;
                var model = JsonConvert.DeserializeObject<LocalizationModel>(content);
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
                foreach (var item in Model.Map)
                {
                    if (key.StartsWith(item.Key))
                        return true;
                }
            }
            return contains;
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

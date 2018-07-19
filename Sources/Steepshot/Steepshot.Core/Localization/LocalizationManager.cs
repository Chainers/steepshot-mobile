using System;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace Steepshot.Core.Localization
{
    public class LocalizationManager
    {
        public const string UpdateUrl = "https://raw.githubusercontent.com/Chainers/steepshot-mobile/master/References/Languages/{0}/dic.xml";

        public LocalizationModel Model { get; }

        public LocalizationManager(LocalizationModel model)
        {
            Model = model;
        }

        public bool Reset(string content)
        {
            try
            {
                var sReader = new StringReader(content);
                var reader = new XmlTextReader(sReader);

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

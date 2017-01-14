using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Sweetshot.Library.Serializing
{
    public interface IJsonConverter
    {
        T Deserialize<T>(string s);
        T DeserializeAnonymousType<T>(string s, T definition);
        string Serialize(object obj);
    }

    public class JsonNetConverter : IJsonConverter
    {
        public JsonNetConverter()
        {
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy()
                    }
                };
                return settings;
            };
        }

        public T Deserialize<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T DeserializeAnonymousType<T>(string s, T definition)
        {
            return JsonConvert.DeserializeAnonymousType(s, definition);
        }
    }
}
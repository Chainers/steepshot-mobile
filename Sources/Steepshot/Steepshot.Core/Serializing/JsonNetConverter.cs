using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp.Portable;

namespace Steepshot.Core.Serializing
{
    public sealed class JsonNetConverter : ISerializer
    {
        private static readonly Encoding _encoding = new UTF8Encoding(false);
        private readonly JsonSerializer _serializer;

        public string ContentType { get; set; }

        public JsonNetConverter()
        {
            ContentType = $"application/json; charset={_encoding.WebName}";
            _serializer = new JsonSerializer();
            Configure(_serializer);
        }

        byte[] ISerializer.Serialize(object obj)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                _serializer.Serialize(writer, obj);
            }

            return output.ToArray();
        }
        
        public string Serialize(object obj)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                _serializer.Serialize(writer, obj);
            }

            return _encoding.GetString(output.ToArray());;
        }

        public T Deserialize<T>(string s)
        {
            var input = new MemoryStream(Encoding.UTF8.GetBytes(s ?? ""));
            using (var reader = new StreamReader(input))
            {
                return _serializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        private void Configure(JsonSerializer serializer)
        {
            serializer.ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };
            serializer.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK";
        }
    }
}
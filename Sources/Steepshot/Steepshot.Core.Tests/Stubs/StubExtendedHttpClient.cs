using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Models.Common;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;
using Steepshot.Core.Clients;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubExtendedHttpClient : ExtendedHttpClient
    {
        protected override async Task<OperationResult<T>> CreateResult<T>(HttpResponseMessage response, CancellationToken token)
        {
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var jObject = JsonNetConverter.Deserialize<JObject>(json);
                var type = typeof(T);

                if (type.IsGenericType && type.Name.StartsWith("ListResponse"))
                {
                    type = type.GenericTypeArguments[0];
                    var rez = jObject["results"].Value<JArray>();
                    jObject = rez[0].Value<JObject>();
                }


                var mNames = GetPropertyNames(type);
                var jNames = jObject.Properties().Select(p => ToTitleCase(p.Name));

                var msg = new List<string>();
                foreach (var pName in jNames)
                {
                    if (!mNames.Contains(pName))
                    {
                        if (pName == "Count" || pName == "Offset" || pName == "TotalCount")
                            Console.WriteLine($"Warning: Missing in model {pName}");
                        else
                            msg.Add($"Missing in model {pName}");
                    }
                }

                foreach (var pName in mNames)
                {
                    if (!jNames.Contains(pName))
                    {
                        if (pName == "Count" || pName == "Offset" || pName == "TotalCount")
                            Console.WriteLine($"Warning: Missing in model {pName}");
                        else
                            msg.Add($"Missing in json {pName}");
                    }
                }

                Assert.IsFalse(msg.Any(), $"Some properties ({msg.Count}) was missed! {Environment.NewLine} {string.Join(Environment.NewLine, msg)}");
            }
            return await base.CreateResult<T>(response, token);
        }

        private HashSet<string> GetPropertyNames(Type type)
        {
            var props = type.GetRuntimeProperties();
            var resp = new HashSet<string>();
            foreach (var prop in props)
            {
                var ignore = prop.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ignore != null)
                    continue;
                var order = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (order != null)
                {
                    resp.Add(order.PropertyName);
                }
                else
                {
                    resp.Add(prop.Name);
                }
            }
            return resp;
        }

        private string ToTitleCase(string name, bool firstUpper = true)
        {
            var sb = new StringBuilder(name);
            for (var i = 0; i < sb.Length; i++)
            {
                if (i == 0 && firstUpper)
                    sb[i] = char.ToUpper(sb[i]);

                if (sb[i] == '_' && i + 1 < sb.Length)
                    sb[i + 1] = char.ToUpper(sb[i + 1]);
            }
            sb.Replace("_", string.Empty);
            var rez = sb.ToString();

            return rez;
        }
    }
}

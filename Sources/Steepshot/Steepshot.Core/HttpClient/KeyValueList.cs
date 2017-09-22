using System.Collections.Generic;

namespace Steepshot.Core.HttpClient
{
    public class KeyValueList : List<KeyValuePair<string, object>>
    {
        public void Add(string key, object value)
        {
            Add(new KeyValuePair<string, object>(key, value));
        }
    }
}

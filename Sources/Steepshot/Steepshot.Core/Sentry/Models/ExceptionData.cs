using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Steepshot.Core.Sentry.Models
{
    public class ExceptionData : Dictionary<string, object>
    {
        private readonly string exceptionType;

        public ExceptionData(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            exceptionType = exception.GetType().FullName;

            foreach (var k in exception.Data.Keys)
            {
                try
                {
                    var value = exception.Data[k];
                    var key = k as string ?? k.ToString();
                    Add(key, value);
                }
                catch (Exception e)
                {
                    //todo nothing
                }
            }

            if (exception.InnerException == null)
                return;

            var exceptionData = new ExceptionData(exception.InnerException);

            if (exceptionData.Count == 0)
            {
                return;
            }

            exceptionData.AddTo(this);
        }

        [JsonProperty("type")]
        public string ExceptionType
        {
            get { return exceptionType; }
        }


        private void AddTo(IDictionary<string, object> dictionary)
        {
            var key = String.Concat(ExceptionType, '.', "Data");
            key = UniqueKey(dictionary, key);
            dictionary.Add(key, this);
        }


        private static string UniqueKey(IDictionary<string, object> dictionary, object key)
        {
            var stringKey = key as string ?? key.ToString();

            if (!dictionary.ContainsKey(stringKey))
                return stringKey;

            for (var i = 0; i < 10000; i++)
            {
                var newKey = String.Concat(stringKey, i);
                if (!dictionary.ContainsKey(newKey))
                    return newKey;
            }

            throw new ArgumentException(String.Format("Unable to find a unique key for '{0}'.", stringKey), "key");
        }
    }
}
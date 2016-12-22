using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Steemix.Library.Serializing
{
    public class JsonUnmarshaller : IUnmarshaller
    {
        public T Process<T>(string response)
        {
            return UnmarshalResponse<T>(response);
        }

        private T UnmarshalResponse<T>(string content)
        {
            try
            {
                var response = JsonConvert.DeserializeObject<T>(content);
                return response;
            }
            catch (Exception ex)
            {
                throw new SerializationException("Fatal error", ex);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Elite.Common.Utilities.ResponseHandling
{
    public static class ResponseMessage
    {
        public static string EncodeData<T>(T data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });

            var bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static T DecodeData<T>(string encodedData)
        {
            var bytes = Convert.FromBase64String(encodedData);
            var json = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
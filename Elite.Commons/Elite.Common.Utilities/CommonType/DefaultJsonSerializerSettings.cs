using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class DefaultJsonSerializerSettings
    {
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static JsonSerializerSettings JSONSettings
        {
            get
            {
                return jsonSettings;
            }
        }
    }
}

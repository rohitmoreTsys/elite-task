using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class File
    {
        [JsonProperty("mimeType")]
        public string mimeType { get; set; }
    }
}

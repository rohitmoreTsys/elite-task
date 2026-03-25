using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class Contents
    {
        public string id { get; set; }

        [JsonProperty("name")]
        public string filename { get; set; }
        [JsonProperty("lastModifiedDateTime")]
        public DateTime lastedited { get; set; }
        public string size { get; set; }
        public Int64 sizeNumber { get; set; }
        [JsonProperty("file")]
        public File file { get; set; }
        public bool isfolder { get; set; }
        public bool isSelected { get; set; }
    }
}

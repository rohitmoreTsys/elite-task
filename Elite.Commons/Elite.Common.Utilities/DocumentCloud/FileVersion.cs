using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class FileVersion
    {
        public string Id { get; set; }
        public string FileId { get; set; }
        public string tag { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string size { get; set; }
    }
}

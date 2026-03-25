using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class FolderContents
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime? lastModifiedDateTime { get; set; }
        public string size { get; set; }
        public Int64 sizeNumber { get; set; }
        public File file { get; set; }
        public string permissions { get; set; }
        public string authorName { get; set; }
        public bool isShared { get; set; }

        public bool isfolder { get; set; }
        public bool isSelected { get; set; }
        public bool isFullPermission { get; set; }
        public bool isViewPermission { get; set; }
        public string CreatedBy { get; set; }
        public string DocumentGUID { get; set; }
        public bool ShowDownloadWarning { get; set; }
        public int VersionNo { get; set; }
    }
}

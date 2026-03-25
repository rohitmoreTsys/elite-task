using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class SharefolderFiles
    {
        public string fileid { get; set; }
        public string fileParent { get; set; }
        public string name { get; set; }
        public string shareid { get; set; }
        public string privatelink { get; set; }
        public string getcontentlength { get; set; }
        public string size { get; set; }
        public DateTime getlastmodified { get; set; }
        public string getcontenttype { get; set; }
        public Int64 sizeNumber { get; set; }
        public bool IsFolder { get; set; }
        public string folderPath { get; set; }
        public string parentFolderName { get; set; }    
        public bool isFullPermission { get; set; }
        public bool isViewPermission { get; set; }
        public string documentGUID { get; set; }
        public bool ShowDownloadWarning { get; set; }
        public string parentFolder { get; set; }
    }
}

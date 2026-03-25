using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class FileFolderDetail
    {
        public string fileId { get; set; }
        public string originalFileId { get; set; }
        public string fileName { get; set; }
        public DateTime lastModifiedDateTime { get; set; }
        public string size { get; set; }
        public string AuthorName { get; set; }
        public string CommitteeId { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class FileFolderDetails
    {
        public string folderPath { get; set; }
        public string fileName { get; set; }
        public string fileGUID { get; set; }
        public string fileOwner { get; set; }
        public bool isPreview { get; set; }
        public string committeeId { get; set; }
        public string folderName { get; set; }
        public string attachmentGUID { get; set; }
        public bool isMeeting { get; set; }
        public string eliteCommitteeId { get; set; }
        public bool isFolder { get; set; }
        public bool isSharedFolder { get; set; }
        public string spaceRef { get; set; }        
    }
}

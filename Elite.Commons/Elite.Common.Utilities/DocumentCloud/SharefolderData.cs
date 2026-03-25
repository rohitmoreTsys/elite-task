using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class SharefolderData
    {
        public string Id { get; set; }
        public string ShareType { get; set; }
        public string UidOwner { get; set; }
        public string DisplaynameOwner { get; set; }
        public string AdditionalInfoOwner { get; set; }
        public string Permissions { get; set; }
        public string Stime { get; set; }
        public string UidFileOwner { get; set; }
        public string DisplaynameFileOwner { get; set; }
        public string AdditionalInfoFileOwner { get; set; }
        public string Path { get; set; }
        public string ItemType { get; set; }
        public string MimeType { get; set; }
        public string StorageId { get; set; }
        public string Storage { get; set; }
        public string ItemSource { get; set; }
        public string FileSource { get; set; }
        public string FileParent { get; set; }
        public string FileTarget { get; set; }
        public string ShareWith { get; set; }
        public string ShareWithUserType { get; set; }
        public string ShareWithDisplayName { get; set; }
        public bool IsFolder { get; set; }
    }
}

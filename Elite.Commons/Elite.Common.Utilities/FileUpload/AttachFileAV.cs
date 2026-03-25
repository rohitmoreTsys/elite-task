using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.FileUpload
{
    public class AttachFileAV
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public bool SizeExceeded { get; set; }
        public bool? VirusDetected { get; set; }
        public bool? IsFileExtensionAllowed { get; set; }


        public long FileSize
        {
            get; set;
        }
    }
}

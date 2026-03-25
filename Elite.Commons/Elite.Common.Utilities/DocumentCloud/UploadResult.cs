using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class UploadResult
    {
        public string FileName { get; set; }
        public bool VirusDetected { get; set; }
        public bool UploadFailed { get; set; }
    }
}

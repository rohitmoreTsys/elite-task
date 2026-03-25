using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.FileUpload
{
    public class AttachFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public long FileSize
        {
            get; set;
        }
    }
}

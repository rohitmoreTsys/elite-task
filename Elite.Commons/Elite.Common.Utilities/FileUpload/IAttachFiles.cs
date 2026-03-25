using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.FileUpload
{
    public interface IAttachFiles
    {
        IList<AttachFile> Files { get; set; }
    }
}

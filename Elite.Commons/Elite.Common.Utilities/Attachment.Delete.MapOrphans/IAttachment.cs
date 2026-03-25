using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.Attachment.Delete.MapOrphans
{
    public interface IAttachment
    {
         long Id { get; set; }
      
         string AttachmentGuid { get; set; }

         string AttachmentDesc { get; set; }

         long? AttachmentSize { get; set; }

         bool IsDeleted { get; set; }
    }
}

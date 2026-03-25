using Elite.Common.Utilities.CommonType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elite.Common.Utilities.Attachment.Delete.MapOrphans
{
    public class AttachmentHelper
    {
        public static IList<T> GetAttachments<T>(IList<T> attachments, AttachmnetType type) where T : IAttachment
        {
            if (attachments?.Count > 0)
            {
                if (type == AttachmnetType.AddAttachment || type == AttachmnetType.MappOrphanAttachment)
                    return attachments.Where(p => p.IsDeleted == false && p.Id == 0).ToList();
                if (type == AttachmnetType.DeleteAttachment)
                    return attachments.Where(p => p.IsDeleted == true).ToList();
                if (type == AttachmnetType.DeleteAttachmentMapping)
                    return attachments.Where(p => p.IsDeleted == true && p.Id > 0).ToList();
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.Attachment.Delete.MapOrphans
{
   public interface IAttachmentService
    {
        void PublishThroughEventBusForDelete(AttachmentDeleteOrMappingEvent evt);

        void PublishThroughEventBusForMapping(AttachmentDeleteOrMappingEvent evt);
    }
}

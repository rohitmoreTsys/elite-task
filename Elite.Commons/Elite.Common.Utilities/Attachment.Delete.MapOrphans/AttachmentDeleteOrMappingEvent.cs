using Elite.Common.Utilities.DocumentCloud;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.Attachment.Delete.MapOrphans
{
  public  class AttachmentDeleteOrMappingEvent
    {

        public AttachmentDeleteOrMappingEvent()
        {
            AttachmentGuids = new List<string>();
        }
        public IList<string> AttachmentGuids
        {
            get; set;
        }
        public List<AttachmentPerson> FullPermissionUsers { get; set; }
    }
}

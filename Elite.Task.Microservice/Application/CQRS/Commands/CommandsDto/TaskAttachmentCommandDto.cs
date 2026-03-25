using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Microsoft.AspNetCore.Http;

namespace Elite_Task.Microservice.Application.CQRS.Commands.CommmandsDto
{
    public class TaskAttachmentCommandDto : IAttachment
    {
        public long TaskID { get; set; }
        public long Id { get; set; }
        public string AttachmentGuid { get; set; }
        public string AttachmentDesc { get; set; }
        public long? AttachmentSize { get; set; }
        public bool IsDeleted { get; set; }
    }
}

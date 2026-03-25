using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto
{
    public class TaskCommentAttachmentDto: IAttachment
    {
        public long commentId { get; set; }
        public long Id { get; set; }
        public string AttachmentGuid { get; set; }
        public string AttachmentDesc { get; set; }
        public long? AttachmentSize { get; set; }
        public bool IsDeleted { get; set; }



    }
}

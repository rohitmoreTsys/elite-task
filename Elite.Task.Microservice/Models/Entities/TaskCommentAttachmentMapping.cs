using Elite_Task.Microservice.Core;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Models.Entities
{
    public class TaskCommentAttachmentMapping: IEntity
    {
        public long Id { get; set; }
        public long commentId { get; set; }
        public string AttachmentGuid { get; set; }
        public string AttachmentName { get; set; }
        public long? AttachmentSize { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public TaskComment Comment { get; set; }



    }
}

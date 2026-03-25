using Elite_Task.Microservice.Core;
using System;
using System.Collections.Generic;

namespace Elite_Task.Microservice.Models.Entities
{

   public partial class TaskAttachmentMapping  : IEntity
    {
        public long Id { get; set; }
        public long? TaskId { get; set; }
        public string AttachmentGuid { get; set; }
        public string AttachmentName { get; set; }
        public long? AttachmentSize { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public EliteTask Task { get; set; }
    }
}

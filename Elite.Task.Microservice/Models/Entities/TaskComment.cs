using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Core;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Models.Entities
{
    public partial class TaskComment : IEntity
    {
        public TaskComment()
        {
            TaskCommentAttachmentMapping = new HashSet<TaskCommentAttachmentMapping>();
        }
        public long Id { get; set; }
        public long? TaskId { get; set; }
        public string Comment { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool? IsActive { get; set; }
        public int? Action { get; set; }
        public EliteTask Task { get; set; }
        public List<TaskCommentAttachmentDto> Attachments { get; set; }

        public ICollection<TaskCommentAttachmentMapping> TaskCommentAttachmentMapping { get; set; }


        public const string PATH_EliteTaskCommentAttachment = "TaskCommentAttachmentMapping";


    }
}

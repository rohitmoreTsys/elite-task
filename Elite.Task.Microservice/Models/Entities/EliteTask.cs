using Elite_Task.Microservice.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elite_Task.Microservice.Models.Entities
{
    public partial class EliteTask : IEntity
    {
        public EliteTask()
        {
            InverseParent = new HashSet<EliteTask>();
            TaskAttachmentMapping = new HashSet<TaskAttachmentMapping>();
            TaskComment = new HashSet<TaskComment>();
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public string Responsible { get; set; }
        public string CoResponsibles { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public long? ParentId { get; set; }
        public string FileLink { get; set; }
        public long? MeetingId { get; set; }
        public DateTime? MeetingDate { get; set; }
        public long? AgendaId { get; set; }
        public long? CommitteeId { get; set; }
        public EliteTask Parent { get; set; }

        public string TaskGuid { get; set; }

        public long SubTaskCount { get; set; }

        public bool? IsActive { get; set; }
        public int? Action { get; set; }
        public int? MeetingStatus { get; set; }
        public bool IsFinalMinutesTasks { get; internal set; } = false;

        public ICollection<EliteTask> InverseParent { get; set; }
        public const string PATH_EliteSubTask = "InverseParent";


        public ICollection<TaskAttachmentMapping> TaskAttachmentMapping { get; set; }
        public const string PATH_EliteTaskAttachment = "TaskAttachmentMapping";

        public ICollection<TaskComment> TaskComment { get; set; }

        public const string PATH_EliteTaskComment = "TaskComment";
        public long? IsNotify { get; set; }

        public bool? IsPublishedToJira { get; set; }
        public string JiraTicketInfo { get; set; }
        public string ClosureComment { get; set; } = String.Empty;
        public string CoResponsibleEmailRecipient { get; set; }
        public string ResponsibleEmailRecipient { get; set; }
        public bool? IsCustomEmailRecipient { get; set; } = false;
        [NotMapped]
        public string topicTitleInEnglish { get; set; }
        [NotMapped]
        public string topicTitleInGerman { get; set; }
        public string ResponsibleDivision { get; set; }
        public DateTime? CompletionDate { get; set; }
    }


    public class EliteTaskD : EliteTask
    {
        public long count { get; set; }

    }

}

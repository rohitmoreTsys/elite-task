using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Application.CQRS.Commands.CommmandsDto;
using System;
using System.Collections.Generic;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class TaskCommand : BaseCommand<long>
    {

        public TaskCommand()
        {
            SubTask = new List<TaskCommand>();
            Attachments = new List<TaskAttachmentCommandDto>();
        }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public List<TaskGroupCommand> CoResponsible { get; set; }
        public TaskPersonCommand Responsible { get; set; }
        public List<TaskGroupCommand> CoResponsibleEmailRecipient { get; set; }
        public TaskPersonCommand ResponsibleEmailRecipient { get; set; }
        public bool? IsCustomEmailRecipient { get; set; } = false;
        public string Description { get; set; }
        public int? Status { get; set; }
        public long? ParentId { get; set; }

        public bool? IsDeleted { get; set; } = false;
        public string FileLink { get; set; }

        public long? MeetingID { get; set; }        
        public DateTime? MeetingDate { get; set; }
        public long? AgendaID { get; set; }
        public long? CommitteeId { get; set; }
        public int? SubTaskCount { get; set; }
        public int? Action { get; set; }
        public int? MeetingStatus { get; set; }
        public bool IsFinalMinutesTasks { get; set; } = false;
        public bool IsDemoUser { get; set; }
        public bool IsCorporateEmail { get; set; } = false;
        public List<TaskCommand> SubTask { get; set; }
        public List<TaskAttachmentCommandDto> Attachments { get; set; }
        public long? notifyUsers { get; set; }
        public Boolean? IsReminder { get; set; }

        public bool IsPublishedToJira { get; set; }
        public JiraTicketInfo JiraTicketInfo { get; set; }
        public string ClosureComment { get; set; }
        public string RejectionComment { get; set; }
        public string topicTitleInEnglish { get; set; }
        public string topicTitleInGerman { get; set; }
        public bool isSubTask { get; set; }
        public string ResponsibleDivision { get; set; }

    }
}

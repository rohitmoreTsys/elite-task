using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public class MeetingMinuteTaskEvent
    {

        public MeetingMinuteTaskEvent()
        {
            Attachments = new List<MeetingTaskAttachmentEvent>();
            Subtask = new List<MeetingMinuteTaskEvent>();
        }

        public long Id { get; set; }
        public int? ActionType { get; set; }
        public long? AgendaTopicId { get; set; }
        public long? MeetingId { get; set; }
        public long? CommitteeId { get; set; }
        public TaskPersonCommand CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public TaskPersonCommand ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public  TaskPersonCommand Responsible { get; set; }
		public List<TaskGroupCommand> CoResponsible { get; set; }
		public long? TaskId { get; set; }
        public int? SubTaskCount { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public string Description { get; set; }
        public string Comments { get; set; }
        public int? Status { get; set; }
        public long? ParentId { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public int? Action { get; set; }
        public List<MeetingMinuteTaskEvent> Subtask { get; set; }
        public List<MeetingTaskAttachmentEvent> Attachments { get; set; }
        public string ClosureComment { get; set; }

    }


    public class MeetingTaskEvent 
    {
        public long RequestId { get; set; }
        public MeetingMinuteTaskEvent Task { get; set; }
    }
}

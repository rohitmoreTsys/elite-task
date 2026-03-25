using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{

    public class TaskEvent : INotificationEvent
    {

        public TaskPersonCommand CreatedBy { get; set; }
        public TaskPersonCommand Responsible { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public string ClosureComment { get; set; }=String.Empty;
        public string RejectionComment { get; set; } = String.Empty;
        public List<string> CommitteeManagerEmailList { get; set; }
        public DateTime? DueDate { get; set; }
        public long? TaskId { get; set; }
        public long? CommitteeId { get; set; } 
        public string CommitteeName { get; set; }
        public string Role { get; set; }
        public string OldResponsible { get; set; }
        public TaskPersonCommand ModifiedBy { get; set; }
        public int? status { get; set; }
        public bool IsDemoUser { get; set; }
        //current implementation 
        // public bool IsMailSent { get; set; }

        /// <summary>
        /// Changes for Json Object
        /// </summary>
        public long? MeetingId { get; set; }
        public string MeetingName { get; set; }
        public string MeetingDatetime { get; set; }
        public Boolean IsConfidential { get; set; }
        public string PoolIdEmailId { get; set; }
        public string PoolIdName { get; set; }
        public string TaskResponsible { get; set; } = string.Empty;
        public string TaskCoResponsible { get; set; } = string.Empty;
        [NotMapped]
        public string topicTitleInEnglish { get; set; } = string.Empty;
        [NotMapped]
        public string topicTitleInGerman { get; set; } = string.Empty;
        [NotMapped]
        public bool isSubTask { get; set; } = false;
        [NotMapped]
        public bool IsUpdate { get; set; } = false;


    }
}

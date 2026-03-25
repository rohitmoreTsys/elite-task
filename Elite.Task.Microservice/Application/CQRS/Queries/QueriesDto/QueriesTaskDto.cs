
using Elite.Common.Utilities.CommonType;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;

namespace Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesTaskDto
    {
        public QueriesTaskDto()
        {
            SubTask = new List<QueriesSubTaskDto>();
            Attachments = new List<QueriesTaskAttachmentDto>();
            Actions = new List<EntityAction>();
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public QueriesPersonDto Responsible { get; set; }
        public List<QueriesGroupDto> CoResponsible { get; set; }
        public List<QueriesGroupDto> CoResponsibleEmailRecipient { get; set; }
        public QueriesPersonDto ResponsibleEmailRecipient { get; set; }
        public bool? IsCustomEmailRecipient { get; set; }
        public QueriesPersonDto CreatedBy { get; set; }
        public string Description { get; set; }
        public string ClosureComment { get; set; }
        public int? Status { get; set; }
        public string FileLink { get; set; }
        public long? CommitteeId { get; set; }
        public long? MeetingId { get; set; }
        public string CommitteeName { get; set; }
        public int CommentCount { get; set; }
        public bool IsFinalMinutesTasks { get; internal set; } = false;
        public int? MeetingStatus { get; set; }
        public IList<EntityAction> Actions { get; set; }
        public IEnumerable<QueriesSubTaskDto> SubTask { get; set; }
        public IEnumerable<QueriesTaskAttachmentDto> Attachments { get; set; }
        //change by vimmal
        public long? notifyUsers { get; set; }

        public QueriesJiraTicketInfoDto JiraTicketInfo { get; set; }

        public bool? IsPublishedToJira { get; set; }
        public int? RoleId { get; set; }
        public string ResponsibleDivision { get; set; }
    }
}

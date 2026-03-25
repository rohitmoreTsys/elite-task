using Elite.Common.Utilities.CommonType;
using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public class TopicHistoryEvent
    {
        public long Id { get; set; }
        public long TopicId { get; set; }
        public TopicHistoryStatus CategoryType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public TaskPersonCommand CreatedBy { get; set; }
        public GroupType? GroupId { get; set; }
        public long? ReferenceId { get; set; }
        public string Comments { get; set; }
		public string AdditionalInfo { get; set; }
		public string TaskClosureComment { get; set; } = String.Empty;

	}

	public class MeetingAgendaEvent
	{
		public long agendaId { get; set; }
		public long meetingId { get; set; }
		public Responsibles responsibleUid { get; set; }
		public List<Responsibles> coResponsibleUid { get; set; }

	}

	public class MeetingAgenda
	{
		public long Id { get; set; }
		public long? TopicId { get; set; }
		public string TopicNo { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime? DueDate { get; set; }
		public string ResponsibleJson { get; set; }
		public string SpeakerJson { get; set; }
		public string GuestJson { get; set; }
		public long? CreatorCommitteeId { get; set; }
		public long? TargetCommitteeId { get; set; }
		public long? MeetingId { get; set; }
		public string MeetingName { get; set; }
		public bool? IsApproved { get; set; }
		public string Status { get; set; }
		public DateTime? CreatedDate { get; set; }
		public DateTime? ModifiedDate { get; set; }
		public string Duration { get; set; }
		public string Comments { get; set; }
		public int? TopicAction { get; set; }
		public int? OrderNo { get; set; }
		public string CreatedBy { get; set; }
		public string ModifiedBy { get; set; }
		public string ApprovedBy { get; set; }
		public int? MinuteActionStatus { get; set; }
		public bool? IsAllowEdit { get; set; }
		public string DescriptionWithoutHtml { get; set; }
		public string CommentsWithoutHtml { get; set; }
		public char TopicType { get; set; }
		public string Notes { get; set; }
		public int? DelegationStatus { get; set; }
		public bool? IsActive { get; set; }
		public int? Action { get; set; }
		public bool? IsTopicDeleted { get; set; }
		public long? HeaderTopicNo { get; set; }
		public string TaskResponsibles { get; set; }

	}

	public class Responsibles
	{

		public Responsibles(string uid)
		{
			this.Uid = uid;
		}

		public string Uid { get; set; }
	}
}

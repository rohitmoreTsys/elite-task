using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;

namespace Elite.Common.Utilities.ListDownload
{
    public class ListToDownloadDto
    {
        public ListToDownloadDto()
        {
            ResponsibleJson = new PersonDto();
            TopicSpeakerJson = new List<PersonDto>();
            TopicGuestJson = new List<PersonDto>();
            TopicResponsibleJson = new List<PersonDto>();
            CoResponsibleJson = new List<QuerieGroupDto>();
            TaskComments = new List<TaskCommentDto>();
        }

        public long Id { get; set; }
        public string Uid { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public char? Gender { get; set; }
        public string DueDate { get; set; }
        public string ScheduledDate { get; set; }

        public PersonDto ResponsibleJson { get; set; }
        public List<QuerieGroupDto> CoResponsibleJson { get; set; }
        public List<TaskCommentDto> TaskComments { get; set; }

        public List<PersonDto> TopicResponsibleJson { get; set; }
        public List<PersonDto> TopicSpeakerJson { get; set; }
        public List<PersonDto> TopicGuestJson { get; set; }
        public string MeetingName { get; set; }
        public string Filelink { get; set; }
        public string CommitteeName { get; set; }

        public string Status { get; set; }

        public int? TopicAction { get; set; }
        public string taskDueStatus { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public int CoreMembers { get; set; }
        public int CommitteeManagers { get; set; }
        public int Users { get; set; }
        public string Requestor { get; set; }
        public string CommitteeType { get; set; }
        public string Description { get; set; }
        public string TopicComments { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string AdditionalInfo { get; set; }
        public string TaskClosureComment { get; set; } = string.Empty;
        public List<PersonDto> MeetingParticipantJson { get; set; }
        public PersonDto Organizer { get; set; }
        public List<ListToDownloadDto> MeetingAgenda { get; set; }
        public string ResponsibleDivision { get; set; }
    }

    public class QuerieGroupDto
    {
        public QuerieGroupDto()
        {
            Users = new List<PersonDto>();
        }

        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public List<PersonDto> Users { get; set; }
    }
    public class PersonDto
    {
        public string Uid { get; set; }
        public string DisplayName { get; set; }
    }

    public enum TaskDueStatus
    {
        Overdue = 0,
        ApproachingDueDate = 1
    }
}
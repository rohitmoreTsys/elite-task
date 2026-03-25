using System;
using System.ComponentModel.DataAnnotations;

namespace Elite.Common.Utilities.CommonType
{
    public class MeetingList
    {
        [Key]
        public long ID { get; set; }
        public long? MeetingNo { get; set; }
        public string MeetingTitle { get; set; }
        public long CommitteeID { get; set; }
        public string Participants { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string MeetingTime { get; set; }
        public string Location { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string Organizer { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? Status { get; set; }
        public DateTimeOffset? StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public bool? IsInvited { get; set; }
        public string SkypeLink { get; set; }
        public string VCDialIn { get; set; }
        public string AgendaBody { get; set; }
        public string DraftAgendaBody { get; set; }
        public string MinuteBody { get; set; }
        public string OutlookBody { get; set; }
        public string AgendaBodyInGerman { get; set; }
        public string DraftAgendaBodyInGerman { get; set; }
        public string MinuteBodyInGerman { get; set; }
        public string OutlookBodyInGerman { get; set; }

        public string CommitteeParticipantGroup { get; set; }

        public bool IsFinalMinutesTasks { get; set; } = false;
        public bool? IsActive { get; set; }
        public int? Action { get; set; }

        public long? TimeZoneID { get; set; }

        public string MeetingParticipantDistribution { get; set; }

        public DateTime? FinalMinutesDate { get; set; }




    }
}

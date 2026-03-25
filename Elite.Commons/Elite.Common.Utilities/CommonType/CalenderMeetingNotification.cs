using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Elite.Common.Utilities.CommonType
{

    
    public class CalenderMeetingNotification
    {
        [Key]
        public int RowNum { get; set; }
        public string ParticipantToList { get; set; }
        public long? MeetingID { get; set; }
        public DateTime? MeetingDate { get; set; }
        public string ParticipantCclist { get; set; }       
        public string ParticipantBcclist { get; set; }
        public string ResponseStatus { get; set; }
        public string DistributionParticipants { get; set; }
        public string ParticipantEmail { get; set; }
        public string CommitteeGroupParticipants { get; set; }
        public string MeetingParticipantDistributionID { get; set; }
        public long? CommitteeID { get; set; }
    }
}
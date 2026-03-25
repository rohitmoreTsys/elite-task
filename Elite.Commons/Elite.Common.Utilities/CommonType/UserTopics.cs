
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class UserTopics
    {
        [Key]
        public long Id { get; set; }
        public string Title { get; set; }
        public string TitleInGerman { get; set; }
        public string ResponsibleJson { get; set; }
        public string SpeakerJson { get; set; }
        public string GuestJson { get; set; }
        public long? TargetCommitteeId { get; set; }
        public long? MeetingId { get; set; }
        public string MeetingName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string Duration { get; set; }
        public string Comments { get; set; }
        public int? TopicAction { get; set; }
        public long? DelegationTargetCommitteeId { get; set; }
        public int? MeetingMode { get; set; }
    }
}

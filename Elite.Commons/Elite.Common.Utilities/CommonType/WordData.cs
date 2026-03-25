using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class WordData
    {
        public MailType DocMailType { get; set; }
        public bool IsConfidential { get; set; }
        public bool IsBilingual { get; set; }
        public string CoreMembers { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingTitleGerman { get; set; }
        public string MeetingDateforFile { get; set; }
        public string MeetingDate { get; set; }
        public string MeetingTime { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }
        public string[] FooterTextEnglish { get; set; }
        public string[] FooterTextGerman { get; set; }
        public List<AgendaData> AgendaList { get; set; }
        public List<AgendaData> AgendaListGerman { get; set; }
        public string MeetingTimeGerman { get; set; }
        

    }
    public class AgendaData
    {
        public string OrderCount { get; set; }
        public string TopicTitle { get; set; }
        public string TopicDescription { get; set; }
        public string Responsibles { get; set; }
        public string Speakers { get; set; }
        public string Guests { get; set; }
        public string ResponsibleLabel { get; set; }
        public string SpeakerLabel { get; set; }
        public string GuestLabel { get; set; }
        public string Duration { get; set; }
        public string TopicTiming { get; set; }
        public List<string> TopicTimingWord { get; set; }
        public string TopicCategory { get; set; }
        public string DocumentCategory { get; set; }
        public string TopicCategoryGerman { get; set; }
        public string DocumentCategoryGerman { get; set; }
        public string TopicType { get; set; }
    }
}

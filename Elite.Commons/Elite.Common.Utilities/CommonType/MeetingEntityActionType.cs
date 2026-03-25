using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum MeetingEntityActionType
    {
        CreateMeeting = 0,
        EditMeeting = 1,
        ViewMeeting = 2,
        RearrangeAgendaTopics = 3,
        SendEmail = 4,
        SendingMeetingInvites = 5,
        UpdatingMeetingInvites = 6,
        CancelMeeting = 7,
        ViewTopic = 8,
        ViewProtocol = 9,
        ShareProtocol = 10,
        DeleteProtocol = 11,
        Add_RemoveTopicsFromMeeting = 12,
        PublishMinutes = 13,
        TransientUser = 14,
        CopyMeeting = 15,
        DeleteMeeting = 16,
        AllowEditCoreMemeber = 17,
        SaveAttendees = 18,
        DownloadAttachment = 19,
        confidential = 20,
        EditMeetingAgenda = 21,
        UploadAgendaAttachment = 22,
        DeleteMeetingAgenda = 23,
        DownloadAgendaAttachment = 24,
        ZipDownload = 25,
        DecisionElement = 26,
        TopicReschedule = 27,
        DownloadMeetingAgenda = 28,
        DownloadDraftMinutes = 29,
        DownloadFinalMinutes = 30,
        DownloadDraftMinutesForAll = 31,
        DownloadFinalMinutesForAll = 32,
        AddExistingTopic = 33,
        ViewAllTopics = 34, 
        HideAllTopics = 35,
        UploadAssignedAgendaAttachment = 36,
        DownloadMultipleMeetingAgendas = 38
    }
}

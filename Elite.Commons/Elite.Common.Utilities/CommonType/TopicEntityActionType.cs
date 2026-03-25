using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum TopicEntityActionType
    {
        CreateTopic = 0,
        EditTopic = 1,
        ViewTopic = 2,
        CreateRFS = 3,
        EditRFS = 4,
        AcceptRejectRFS = 5,
        Delegate = 6,
        ViewHistory = 7,
        AddToMeeting = 8,
        Resubmit = 9,
        AcceptRejectRFD = 10,
        CreateRFD = 11,
        CopyTopic = 12,
        Complete = 13,
        Delete = 14,
        EditResponsible = 15,
        AllowSpeakerGuestEdit = 16,
        AllowScheduleTopicEdit = 17,
        UploadAttachment = 18,
        DownloadAttachment = 19,
        DownloadPDFandExcel = 20,
        Annotation = 21
    }
}

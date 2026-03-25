using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum TopicAction
    {
        Created = 1,
        RFS = 2,
        Accepted = 3,
        Rejected = 4,
        Resubmission = 5,
        Delegation = 6,
        AddToMeeting = 7,
        AddTopicInMeeting = 8,
        RFD = 9,
        DelegatedAndScheduled = 10,
        Copied = 11,
        CopiedCopyTopic = 12,
        CopiedCloneTask = 13

    }

    public enum MeetingDelegationStatus
    {
        Open = 1,
        RFD = 2,
        Accepted = 3,
        Rejected = 4
    }
}

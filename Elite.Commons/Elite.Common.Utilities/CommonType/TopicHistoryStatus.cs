using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public enum TopicHistoryStatus
    {
        Created = 1,
        
        Requested = 2,
        RFSCreated = 3,
        CreateAddToMeeting = 4,
        AddToMeeting = 5,
        RFSApproved = 6,
        RFSRejected = 7,
        Dlegated = 8,
        Completed = 9,
        DeletedFromMeeting = 10,
        Resubmitted = 11,
        RequestedDelegation =12,
        RFDApproved = 13,
        RFDRejected = 14,
        RemovedFromMeeting =15,
        InformationDecisionAdded =16,
        AcceptDecisionAdded=17,
        RejectDecisionAdded = 18,
        DecidedDecisionAdded=19,
        Taskcreated = 20,
        DelegationDecisionAdded=21,
        SubTaskCreated =22,
        InformationDecisionDeleted=23,
        AcceptDecisionDeleted=24,
        RejectDecisionDeleted=25,
        DecidedDecisionDeleted = 26,
        TaskDeleted=27,
        SubTaskDeleted=28,
        ReOpen = 29,
        DelegatedAndScheduled = 30,
        TopicDelete = 31,
        Rescheduled = 32,
        Copied = 33,
        CopiedCopyTopic = 34,
        CopiedCloneTask = 35,
        InformationDecisionUpdated = 36,
        AcceptDecisionUpdated=37,
        RejectDecisionUpdated=38,
        DecidedDecisionUpdated=39,
        TaskUpdated=40,
        SubTaskUpdated=41,
        Updated = 42,
        DecisionOrderChanged=43
    }
}

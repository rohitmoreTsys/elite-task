using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public class MeetingTaskAttachmentEvent
    {
        public long id { get; set; }
        public long minuteActionMapping { get; set; }
        public string attachmentGuid { get; set; }
        public string attachmentDesc { get; set; }
        public DateTime? createdDate { get; set; }
        public TaskPersonCommand createdBy { get; set; }
        public long? attachmentSize { get; set; }
        public bool Isdeleted { get; set; }

    }
}

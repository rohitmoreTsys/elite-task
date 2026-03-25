using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public class MeetingMinuteTaskDeleteEvent
    {
        public List<long> TaskIds { get; set; }
        public TaskPersonCommand ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}

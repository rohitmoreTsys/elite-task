using Elite_Task.Microservice.Application.CQRS.Commands;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class MeetingTaskDeleteCommand : IRequest<bool> {
        public List<long> TaskIds { get; set; }
        public bool IsDeleteFromMeeting { get; set; }
        public bool IsDemoUser { get; set; }
    }
  
}

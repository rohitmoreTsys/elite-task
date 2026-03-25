using Elite_Task.Microservice.Application.CQRS.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Topic.Microservice.Application.CQRS.Commands
{
    public class TaskPublishedAndUnPublishedCommand : BaseCommand<bool>
    {

        public TaskPublishedAndUnPublishedCommand()
        {
            Tasks = new List<MeetingTaskEvents>();
        }
        public string MeetingName { get; set; }
        public int MeetingStatus { get; set; }
        public IList<MeetingTaskEvents> Tasks { get; set; }
    }

    public class MeetingTaskEvents
    {
        public long Id { get; set; }

    }
}

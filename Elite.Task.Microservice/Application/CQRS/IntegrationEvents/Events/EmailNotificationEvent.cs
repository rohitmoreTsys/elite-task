using Elite.Task.Microservice.CommonLib;
using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public class EmailNotificationEvent : BaseNotificationEvent
    {
        public Guid NotificationID { get; set; }

        public Boolean IsReminder { get; set; }

    }
}

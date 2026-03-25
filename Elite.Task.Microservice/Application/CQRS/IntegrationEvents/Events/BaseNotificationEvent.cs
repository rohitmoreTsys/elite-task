using Elite.Common.Utilities.CommonType;
using Elite.Task.Microservice.CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events
{
    public abstract class  BaseNotificationEvent
    {         

        public INotificationEvent Message { get; set; }

        public GroupType GroupID { get; set; }

       
        public NotificationActionType ActionType { get; set; }

		public long TaskId { get; set; }

		public string Description { get; set; }

		public DateTime? DueDate { get; set; }

       

    }
}

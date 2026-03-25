using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class SendNotificationCommand
    {
        public long Id { get; set; }
        public long Sourcetypeid { get; set; }
        public int GroupId { get; set; }
        public int ActionType { get; set; }
        public string JsonMessage { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public bool? IsFailed { get; set; }
        public bool? IsProcessed { get; set; }
        public int? NotificationType { get; set; }
        public DateTimeOffset? ProcessedDate { get; set; }
    }
}

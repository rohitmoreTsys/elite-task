using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.EventBus.EventStore
{
   public class EliteEventStoreDto
    {
        public long Id { get; set; }
        public long? Sourcetypeid { get; set; }
        public int GroupId { get; set; }
        public int ActionType { get; set; }
        public string JsonMessage { get; set; }
        public DateTimeOffset CreatedDate { get; set; }      
        public bool? IsProcessed { get; set; }
        public int? NotificationType { get; set; }        
        public Boolean IsReminder { get; set; }
    }
}

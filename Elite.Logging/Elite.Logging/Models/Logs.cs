using System;
using System.Collections.Generic;

namespace Elite.Logging.Models
{
    public partial class Logs
    {
        public long LogId { get; set; }
        public string LogDescription { get; set; }
        public string LogUserId { get; set; }
        public DateTime? LogDateTime { get; set; }
        public string LogServiceName { get; set; }
    }
}

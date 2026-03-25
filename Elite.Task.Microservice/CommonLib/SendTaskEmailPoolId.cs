using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class SendTaskEmailPoolId
    {
        public string PoolIdEmailId { get; set; }
        public string PoolIdName { get; set; }
        public IList<SendEmail> SMPTPMailList { get; set; }
    }
}

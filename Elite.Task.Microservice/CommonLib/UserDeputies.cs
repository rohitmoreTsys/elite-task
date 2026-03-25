using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class UserDeputies
    {
        public long Id { get; set; }
        public string CommitteeName { get; set; }
        public long CommitteeId { get; set; }
        public string DeputyUid { get; set; }
        public string EmailId { get; set; }
    }
}

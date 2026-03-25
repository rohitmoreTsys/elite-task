using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class CommitteeDetail
    {
        public long CommitteeId { get; set; }
        public string CommitteeName { get; set; }
        public string PoolIdEmailId { get; set; }
        public string PoolIdName { get; set; }
        public bool PoolIdIsActive { get; set; }
    }
}

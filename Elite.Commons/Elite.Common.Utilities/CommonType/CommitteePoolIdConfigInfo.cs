using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class CommitteePoolIdConfigInfo
    {
        public long CommitteeId { get; set; }
        public bool PoolIdIsActive { get; set; }
        public string PoolIdEmailId { get; set; }
        public string PoolIdName { get; set; }
        public string VaultPath { get; set; }
    }
}

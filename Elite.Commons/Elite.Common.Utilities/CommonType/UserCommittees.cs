using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
   public class UserCommittees
    {
        [Key]
        public long? CommitteeId { get; set; }
        public string CommitteeName { get; set; }
    }
}

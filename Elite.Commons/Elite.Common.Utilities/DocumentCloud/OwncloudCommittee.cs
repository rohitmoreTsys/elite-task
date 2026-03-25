using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class OwncloudCommittee
    {
        public string committeeId { get; set; }
        public string committeeName { get; set; }
        public List<string> shareList { get; set; }
        public bool IsSharedFolder { get; set; }
    }
}

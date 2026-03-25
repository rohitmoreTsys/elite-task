using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    /// <summary>
    /// We have used this class for meeting service to get the details from the services
    /// </summary>
    public class MeetingInfo
    {
        public string MeetingId { get; set; }
        public string MeetingName { get; set; }
        public string MeetingDate { get; set; }
        public Boolean IsConfidential { get; set; }

    }
}

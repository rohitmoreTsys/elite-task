using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class DeleteObject
    {
        public string[] items { get; set; }
        public string committeeId { get; set; }
        public string fileOwner { get; set; }
        public bool isFromMeeting { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Elite.Auth.Token.Lib.Entities
{
    public partial class TrackRequestSessions
    {
        public Guid RequestId { get; set; }
        public DateTime? CreateDate { get; set;}
    }
}

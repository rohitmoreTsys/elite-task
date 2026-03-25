using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Models.Entities
{
    public partial class LegalPropertyContent
    {
        public int Id { get; set; }
        public string  BundleId { get; set; }
        public string Version { get; set; }
        public bool IsInReview { get; set; }

    }
}

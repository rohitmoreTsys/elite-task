using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class OwncloudUser
    {
        public string onPremisesSamAccountName { get; set; }
        public string displayName { get; set; }
        public string mail { get; set; }
        public PasswordProfile passwordProfile { get; set; }
    }
}

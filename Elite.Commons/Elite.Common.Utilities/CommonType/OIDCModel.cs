using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
   public class OIDCModel
    {
        // public string AccessCode { get; set; }
        public string id_token { get; set; }

        public string refresh_token { get; set; }
        public string userId { get; set; }
        public string access_token { get; set; }
        public int TokenExpires { get; set; }
    }
}

using Elite.Auth.Token.Lib.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Command
{
    public class AuthTokenCommand
    {
        public Guid Id { get; set; }
        public string Uid { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IDToken { get; set; }
        public DateTime TokenExpireDateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public int SourceId { get; set; }
        public bool? IsRefreshTokenGenerated { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Auth.Token.Lib.Common
{
    public class AuthReponse
    {
        public Guid Id { get; set; }
        public string Uid { get; set; }
        public bool IsTokenRefresh { get; set; } = false;
    }
}

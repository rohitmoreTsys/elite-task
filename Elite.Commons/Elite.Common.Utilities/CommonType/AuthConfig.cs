using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class AuthConfig
    {
        public string RedirectUrl { get; set; }
        public string TokenUrl { get; set; }
        public string JWKSVal { get; set; }
        public string UserInfoUrl { get; set; }
        public string Introspection { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string ClientSecret { get; set; }
        public string IntrospectionSecret { get; set; }
        public string IntrospectionId { get; set; }
        public string EncryptionKey { get; set; }
        public double CookieTime { get; set; }
        public int ServiceId { get; set; }
    }
}

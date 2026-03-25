using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Primitives;

namespace Elite.OIDC.Handler.Lib.Model
{
    public class AuthSchemeOptions : CookieAuthenticationOptions// AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "OIDCScheme";
        public string Scheme => DefaultScheme;
        public StringValues AuthKey { get; set; }

    }
}


using Elite.OIDC.Handler.Lib.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.OIDC.Handler.Lib.Auth
{
    public static class AuthBuilder
    {
        public static AuthenticationBuilder Creator(this AuthenticationBuilder builder, Action<AuthSchemeOptions> configureOptions = null)
        {

            // Add custom authentication scheme with custom options and custom handler
            return builder.AddScheme<AuthSchemeOptions, AuthHandler>(AuthSchemeOptions.DefaultScheme, configureOptions);
        }
    }
}

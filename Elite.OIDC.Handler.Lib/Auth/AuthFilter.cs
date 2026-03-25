using Elite.OIDC.Handler.Lib.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.OIDC.Handler.Lib.Auth
{
    public class AuthFilter : IAsyncAuthorizationFilter
    {
        public AuthorizationPolicy Policy { get; }
        public IConfiguration _config { get; private set; }

        public AuthFilter(AuthorizationPolicy policy, IConfiguration config)
        {
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _config = config;
        }

        // public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)

        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Allow Anonymous skips all authorization
            if (context.Filters.Any(item => item is IAllowAnonymousFilter))
            {
                return;
            }

            var policyEvaluator = context.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
            var authenticateResult = await policyEvaluator.AuthenticateAsync(Policy, context.HttpContext);
            var authorizeResult = await policyEvaluator.AuthorizeAsync(Policy, authenticateResult, context.HttpContext, context);

            if (authorizeResult.Challenged)
            {
                bool redirect = Convert.ToBoolean(_config.GetSection("redirect").Value);
                // Return custom 401 result
                context.Result = new UnAuthResult("Authorization failed.", redirect);
            }
            else if (authorizeResult.Forbidden)
            {
                // Return default 403 result
                context.Result = new ForbidResult(Policy.AuthenticationSchemes.ToArray());
            }
        }


    }
}

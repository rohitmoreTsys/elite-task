using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Elite.OIDC.Handler.Lib.Model
{
    internal class UnAuthResult : JsonResult
    {
        public UnAuthResult(string message, bool redirect)
            : base(new AuthError(message, redirect))
        {
            if (redirect)
                StatusCode = StatusCodes.Status511NetworkAuthenticationRequired;
            else
                StatusCode = StatusCodes.Status203NonAuthoritative;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.OIDC.Handler.Lib.Model
{
    internal class AuthError
    {
        private string message;

        public AuthError(string message, bool redirect)
        {
            this.message = message;
        }
    }

}

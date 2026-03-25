using Elite.Common.Utilities.Encription;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Elite.OIDC.Handler.Lib.Auth
{
   public class AuthWrapper
    {
         public static string ReplcaeSpecialCharacter(string userId) =>
            Regex.Replace(userId, "[^a-zA-Z0-9]+", string.Empty);
        
    }
}

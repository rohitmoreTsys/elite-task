using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
   public static class StringToUpper
    {
        public static string Upper(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return str.ToString().ToUpper();
            return null;
        }
    }
}

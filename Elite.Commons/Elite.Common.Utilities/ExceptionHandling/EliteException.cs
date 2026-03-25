using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.ExceptionHandling
{
    public class EliteException : System.Exception
    {
        public EliteException()
        { }

        public EliteException(string message)
            : base(message)
        { }

        public EliteException(string message, System.Exception innerException)
            : base(message, innerException)
        { }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.ExceptionHandling
{
  public  class Authorization401Exception : Exception
    {
        public Authorization401Exception()
        { }

        public Authorization401Exception(string message)
            : base(message)
        { }

        public Authorization401Exception(string message, System.Exception innerException)
            : base(message, innerException)
        { }
    }
}

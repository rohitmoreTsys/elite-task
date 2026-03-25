using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class ExceptionFormator
    {
        public static string FormatExceptionMessage(Exception exception)
        {
            return $"{(exception.Message)}\t{(exception.StackTrace)})}}";
        }

        private string FormatExceptionMessage(Exception exception, string _controllerName, string _actionname, string _displayName)
        {
            return $"{(_controllerName)}\t{(_actionname)}\t{(_displayName)}\t{(exception.Message)}\t{(exception.StackTrace)}\t{(exception.InnerException != null ? exception.InnerException.Message : string.Empty)}";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Elite.Common.Utilities.ExceptionHandling
{

      public interface IJsonErrorResponse
    {
         List<ErrorMessage> Messages { get; set; }
        object Message { get; set; }
         HttpStatusCode StatusCode { get; set; }
    }

    public class JsonErrorResponse : IJsonErrorResponse
    {
        public JsonErrorResponse()
        {
            Messages = new List<ErrorMessage>();
        }
        public List<ErrorMessage> Messages { get; set; }
        public object Message { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    } 
}

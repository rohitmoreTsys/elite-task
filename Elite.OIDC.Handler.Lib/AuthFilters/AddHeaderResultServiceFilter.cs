using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.OIDC.Handler.Lib.AuthFilters
{
    public class AddHeaderResultServiceFilter : IResultFilter
    {
        private readonly IConfiguration _configuration;
        public AddHeaderResultServiceFilter(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            //if (Convert.ToBoolean(_configuration.GetSection("UseOIDC").Value))
            //{
            //    RemoveHeadersFromResponse(context.HttpContext.Response, "uid");
            //    //RemoveHeadersFromResponse(context.HttpContext.Response, "DeputyUID");
            //}
        }

        public void OnResultExecuted(ResultExecutedContext context) { }

        private void RemoveHeadersFromResponse(HttpResponse httpResponse, string key)
        {
            if (httpResponse.Headers.ContainsKey(key))
            {
                httpResponse.Headers.Remove(key);
            }
        }

    }
}


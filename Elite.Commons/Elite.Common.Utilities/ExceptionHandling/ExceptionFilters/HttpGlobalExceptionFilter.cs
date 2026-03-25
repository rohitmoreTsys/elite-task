using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Elite.Common.Utilities.ExceptionHandling.ExceptionFilters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly string _errorMessage;
        private readonly IHostingEnvironment env;
        private const string _systemErrorMessage = "SystemExceptionMessage";
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;
        public HttpGlobalExceptionFilter(IHostingEnvironment env, IConfiguration configuration, ILogger<HttpGlobalExceptionFilter> logger)
        {

            this.env = env;
            _errorMessage = configuration.GetSection(_systemErrorMessage).Value;
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            var type = context.Exception.Source;
            string _controllerName = string.Empty;
            string _actionName = string.Empty;
            string _displayName = string.Empty;
            if (context.ActionDescriptor != null)
            {
                _controllerName = ((ControllerActionDescriptor)context.ActionDescriptor).ControllerName;
                _actionName = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName;
                _displayName = context.ActionDescriptor.DisplayName;
               
            }

            _logger.LogError(FormatExceptionMessage(context.Exception, _controllerName, _actionName, _displayName));

            IJsonErrorResponse errorMessage = new JsonErrorResponse();
            if (context.Exception.GetType() == typeof(EliteException))
            {
                if (context.Exception.InnerException != null)
                {
                    var response = ExceptionFactory.GetJsonErrorResponseFactory(context.Exception.InnerException);
                    if (response != null)
                        errorMessage = response;
                }

                errorMessage.Message = context.Exception.Message;
                errorMessage.StatusCode = HttpStatusCode.BadRequest;
                context.Result = new BadRequestObjectResult(errorMessage);
                context.HttpContext.Response.StatusCode = (int)errorMessage.StatusCode;

            }
            else
            {
                if (env.IsDevelopment())
                    errorMessage.Message = context.Exception;
                else
                {
                    if(context.Exception.GetType() == typeof(ArgumentException))
                    {
                        errorMessage.Message = "BadRequest";
                        errorMessage.StatusCode = HttpStatusCode.BadRequest;
                        context.Result = new BadRequestObjectResult(errorMessage);
                        context.HttpContext.Response.StatusCode = 400;
                    }
                    else
                    {
                        errorMessage.Messages.Add(new ErrorMessage
                        {
                            PropertyName = context.Exception.GetType().Name,
                            Message = "System Exception"
                        });
                        //errorMessage.Message = context.Exception;
                        errorMessage.Message = _errorMessage;
                        errorMessage.StatusCode = HttpStatusCode.InternalServerError;
                        context.Result = new InternalServerErrorObjectResult(errorMessage);
                        context.HttpContext.Response.StatusCode = (int)errorMessage.StatusCode;
                    }
                }
            }

            context.ExceptionHandled = true;
        }

        private string FormatExceptionMessage(Exception exception, string _controllerName, string _actionname, string _displayName)
        {
            string stackTrace = Regex.Replace(exception.StackTrace, @"\t|\n|\r", "");

            return $"{(_controllerName)}\t{(_actionname)}\t{(_displayName)}\t{(exception.Message)}\t{(stackTrace)}\t{(exception.InnerException != null ? exception.InnerException.Message : string.Empty)}";
        }

        private string FormatExceptionMessage(Exception exception)
        {
            return $"{(exception.Message)}\t{(exception.StackTrace)})}}";
        }
    }
}

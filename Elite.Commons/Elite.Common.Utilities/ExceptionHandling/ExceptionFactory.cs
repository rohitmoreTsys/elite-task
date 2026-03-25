using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation;

namespace Elite.Common.Utilities.ExceptionHandling
{
    public static class ExceptionFactory
    {
        public static IJsonErrorResponse GetJsonErrorResponseFactory(Exception exception)
        {
            var errorMessage = new JsonErrorResponse();

            //Fluent Validation Exception
            if (exception.GetType() == typeof(ValidationException))
            {
                var list = ((ValidationException)exception).Errors.Select(a => new ErrorMessage()
                {
                    Message = a.ErrorMessage,
                    PropertyName = a.PropertyName
                })
                .ToList<ErrorMessage>();

                errorMessage.Message = ((ValidationException)exception).Message;

                if (list?.Count > 0)
                    errorMessage.Messages.AddRange(list);

                return errorMessage;
            }
            //if any other custom exception like  :-
            //validation exception
            //UnauthorizedAccessException

            return null;
        }
    }
}

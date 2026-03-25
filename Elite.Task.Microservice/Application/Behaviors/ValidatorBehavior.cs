using Elite.Common.Utilities.ExceptionHandling;
//using Elite_Task.Microservice.Application.Exceptions;
using FluentValidation;

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.Behaviors
{
    public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IValidator<TRequest>[] _validators;
        public ValidatorBehavior(IValidator<TRequest>[] validators) => _validators = validators;
             

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var failures = _validators
              .Select(v => v.Validate(request))
              .SelectMany(result => result.Errors)
              .Where(error => error != null)
              .ToList();

            if (failures.Any())
            {
                throw new EliteException($" Validation Error for type {typeof(TRequest).Name}", new ValidationException("Validation exception", failures));
            }

            var response = next();
            return response;
        }
    }
}

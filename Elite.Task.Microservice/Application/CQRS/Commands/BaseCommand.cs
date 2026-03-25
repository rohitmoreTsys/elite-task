using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public abstract  class BaseCommand<R> : IRequest<R>
    {       
        public long Id { get; set; }
    }
}

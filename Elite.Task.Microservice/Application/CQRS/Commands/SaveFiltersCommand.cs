using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class SaveFiltersCommand : IRequest<bool>
    {
        public string FilterJson { get; set; }
        public bool ClearFilters { get; set; }
    }
}

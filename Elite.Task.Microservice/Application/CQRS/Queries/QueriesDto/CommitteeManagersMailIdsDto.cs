using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class CommitteeManagersMailIdsDto
    {
        public string RequestedBy { get; set; }

        public string CreatedBy { get; set; }

        public List<string> CommitteeMailIds { get; set; }
    }
}



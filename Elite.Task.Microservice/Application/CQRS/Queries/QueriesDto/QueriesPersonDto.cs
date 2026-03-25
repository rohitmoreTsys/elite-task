using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesPersonDto
    {
        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
    }
}

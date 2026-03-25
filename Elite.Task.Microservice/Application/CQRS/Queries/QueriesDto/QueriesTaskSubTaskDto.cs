using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries
{
    public class QueriesTaskSubTaskDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public QueriesPersonDto Responsible { get; set; }
        public QueriesPersonDto CreatedBy { get; set; }
        public int? Status { get; set; }
        public bool HasSubTask { get; set; }
    }
}

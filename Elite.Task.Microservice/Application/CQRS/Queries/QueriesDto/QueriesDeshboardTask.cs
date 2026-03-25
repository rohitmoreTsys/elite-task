using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesDeshboardTask
    {
        public long Id { get; set; }
        public long? ParentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public QueriesPersonDto Responsible { get; set; }
		public List<QueriesGroupDto> CoResponsible { get; set; }
		public QueriesPersonDto CreatedBy { get; set; }
    }
}

using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{

    public class QueriesSubTaskPDFTaskDto
    {
       
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public QueriesPersonDto Responsible { get; set; }
		public List<QueriesGroupDto> CoResponsible { get; set; }
		public string Description { get; set; }
        
    }
    public class QueriesPDFTaskDto : QueriesSubTaskPDFTaskDto
    {
        public QueriesPDFTaskDto()
        {
            SubTask = new List<QueriesPDFTaskDto>();
        }
        public QueriesPersonDto CreatedBy { get; set; }
        public IEnumerable<QueriesSubTaskPDFTaskDto> SubTask { get; set; }
    }
}

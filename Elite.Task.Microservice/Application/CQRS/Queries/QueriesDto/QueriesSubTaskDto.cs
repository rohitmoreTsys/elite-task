using Elite.Common.Utilities.CommonType;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesSubTaskDto
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime? DueDate { get; set; }
        public QueriesPersonDto Responsible { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public long? ParentId { get; set; }

        public QueriesPersonDto CreatedBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public QueriesPersonDto ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
        public int CommentCount { get; set; }
        public IList<EntityAction>  Actions  { get; set; }
    }
}

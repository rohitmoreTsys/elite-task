using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{  
    public class QueriesCommentDto
    {
        public QueriesCommentDto()
        {
            Attachments = new List<QueriesTaskAttachmentDto>();
        }
        public long Id { get; set; }
        public long? TaskId { get; set; }
        public string Comment { get; set; }
        public QueriesPersonDto CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public QueriesPersonDto ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public IEnumerable<QueriesTaskAttachmentDto> Attachments { get; set; }
    }
}

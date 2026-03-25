using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesTaskAttachmentDto 
    {
        public long Id { get; set; }
        public string AttachmentDesc { get; set; }
        public string AttachmentGuid { get; set; }
        public long? AttachmentSize { get; set; }
        public bool IsDeleted { get; set; }
    }
}

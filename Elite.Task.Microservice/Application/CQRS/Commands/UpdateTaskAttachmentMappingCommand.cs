using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.JiraEntities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class UpdateTaskAttachmentMappingCommand : IRequest<long>, IAttachment
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long TaskId { get; set; }
        [DataMember]
        public string AttachmentGuid { get; set; }
        [DataMember]
        public string AttachmentName { get; set; }
        [DataMember]
        public string AttachmentDesc{ get; set; }
        [DataMember]
        public long? AttachmentSize { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }

        public UpdateTaskAttachmentMappingCommand(long id, long agendaId, string attachmentGuid, string attachmentDesc, DateTime? createdDate, LookUpFields createdBy, long? attachmentSize,bool isDeleted)
        {
            Id = id;
            TaskId = agendaId;
            AttachmentGuid = attachmentGuid;
            AttachmentName = attachmentDesc;
            AttachmentSize = attachmentSize;
            IsDeleted = isDeleted;
        }
    }
 }

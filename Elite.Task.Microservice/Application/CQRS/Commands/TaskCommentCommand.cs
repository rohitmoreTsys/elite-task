using System;
using System.Collections.Generic;
using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite_Task.Microservice.Application.CQRS.Commands;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class TaskCommentCommand : BaseCommand<long>
    {
        public long? TaskId { get; set; }
        public string Comment { get; set; }
        public List<TaskCommentAttachmentDto> Attachments { get; set; }
    }
}

using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.CommonType;
using Elite.Task.Microservice.Repository;
using Elite.Task.Microservice.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elite.Task.Microservice.Repository.Contracts;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Common.Utilities.RequestContext;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class UpdateTaskAttachmentMappingCommandHandler : IRequestHandler<UpdateTaskAttachmentMappingCommand, long>
    {

        private readonly ITaskAttachmentRepository _taskAttachmentRepository;
        private readonly IMediator _mediator;
        private readonly Func<IConfiguration, IAttachmentService> _attachmentServiceFactory;
        private readonly IAttachmentService _attachmentService;
        protected readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly string securedUID = string.Empty;


        public UpdateTaskAttachmentMappingCommandHandler(IMediator mediator, ITaskAttachmentRepository taskAttachmentRepository, IConfiguration configuration, Func<IConfiguration, IAttachmentService> attachmentServiceFactory, 
            IRequestContext requestContext, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory)
        {

            _taskAttachmentRepository = taskAttachmentRepository ?? throw new ArgumentNullException(nameof(taskAttachmentRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _attachmentServiceFactory = attachmentServiceFactory;
            _attachmentService = _attachmentServiceFactory(configuration);
            _configuration = configuration;
            securedUID = requestContext.DecrpUID;
            _userServiceFactory = userServiceFactory;
            _userService = _userServiceFactory(configuration, requestContext);
        }

        public async Task<long> Handle(UpdateTaskAttachmentMappingCommand message, CancellationToken cancellationToken)
        {
            long result = 0;
            try
            {
                if (message != null)
                {
                    TaskAttachmentMapping taskAttachment = GetTaskAttachmentMappingObject(message);

                    if (taskAttachment != null)
                    {
                        taskAttachment = _taskAttachmentRepository.UpdateAttachment(taskAttachment);
                        await _taskAttachmentRepository.UnitOfWork.SaveEntitiesAsync();
                        result = taskAttachment.Id;
                    }

                }

            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        private TaskAttachmentMapping GetTaskAttachmentMappingObject(UpdateTaskAttachmentMappingCommand message)
        {
            TaskAttachmentMapping topicAttachment = new TaskAttachmentMapping();
            topicAttachment.Id = message.Id;
            topicAttachment.TaskId = message.TaskId;
            topicAttachment.CreatedDate = DateTime.Now;
            topicAttachment.AttachmentSize = message.AttachmentSize;
            topicAttachment.AttachmentName = message.AttachmentName;
            topicAttachment.AttachmentGuid = message.AttachmentGuid;
            topicAttachment.CreatedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
            if (topicAttachment.CreatedBy == "null")
            {
                topicAttachment.CreatedBy = null;
            }
            return topicAttachment;
        }

    }
}

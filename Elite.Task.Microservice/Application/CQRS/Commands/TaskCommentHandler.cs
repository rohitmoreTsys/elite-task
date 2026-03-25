using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.CommonLib;
using Elite.Common.Utilities.ExceptionHandling;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class TaskCommentHandler : BaseCommandHandler<TaskCommentCommand>, IRequestHandler<TaskCommentCommand, long>
    {
        protected readonly IMediator _mediator;
        protected readonly ICommentRepository _repository;
        protected readonly IConfiguration _configuration;
        private readonly IAttachmentService _attachmentService;
        private readonly Func<IConfiguration, IAttachmentService> _attachmentServiceFactory;
        protected readonly ITaskRepository _taskRepository;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;
        private string pUID;
        private readonly string securedUID = string.Empty;

        public TaskCommentHandler(IMediator mediator, ICommentRepository repository, IConfiguration configuration, Func<IConfiguration, IAttachmentService> attachmentServiceFactory,
                        ITaskRepository taskrepository, IRequestContext requestContext, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory)
        {
            _mediator = mediator;
            _repository = repository;
            _configuration = configuration;
            this._attachmentServiceFactory = attachmentServiceFactory;
            this._attachmentService = this._attachmentServiceFactory(this._configuration);
            _taskRepository = taskrepository;
            _userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            this.pUID = requestContext.IsDeputy ? requestContext.DeputyUID != null ? requestContext.DeputyUID.Upper() : string.Empty
                                                : requestContext.UID != null ? requestContext.UID.ToUpper() : string.Empty;
            securedUID = requestContext.DecrpUID;
        }


        public async Task<long> Handle(TaskCommentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await SaveandUpdateComments(request);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<long> SaveandUpdateComments(TaskCommentCommand request)
        {
            if (request.TaskId.HasValue)
            {
                var task = await _taskRepository.GetByIdAsync(request.TaskId.Value);
                CheckRoles(task);
            }

            TaskComment comment = new TaskComment();
            List<string> delAttachments = new List<string>();
            comment.Comment = request.Comment;
            comment.TaskId = request.TaskId;
            comment.Id = request.Id;
            comment.CreatedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
            comment.CreatedDate = DateTime.Now;
            comment.ModifiedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
            comment.ModifiedDate = DateTime.Now;
            CreateCommentAttachmentMapping(comment, comment.Id, request.Attachments, JsonConvert.DeserializeObject<TaskPersonCommand>(comment.CreatedBy), delAttachments);

            try
            {
                if (IsUpdate(comment.Id))
                    UpdateComment(comment, delAttachments);
                else
                    Create(comment);
                await _repository.UnitOfWork.SaveEntitiesAsync();

                //publishing to kafka ---> to delete the attachment from Attachment service
                PublishThroughEventBusForDeleteAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.DeleteAttachment));

                //publishing to kafka ---> to set IsOrphen to false for mapping topic attachment
                PublishThroughEventBusForMappingAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.MappOrphanAttachment));
            }
            catch (Exception ex)
            {
                throw;
            }
            return comment.Id;
        }

        private void CreateCommentAttachmentMapping(TaskComment comment, long taskId, IList<TaskCommentAttachmentDto> attachments, TaskPersonCommand user, List<string> delAttachments)
        {
            if (attachments?.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment.IsDeleted == false)
                    {
                        comment.TaskCommentAttachmentMapping.Add(AttachmentMapperAsync(comment, attachment, user));
                    }
                    else
                        delAttachments.Add(attachment.AttachmentGuid);
                }
            }
        }

        private TaskCommentAttachmentMapping AttachmentMapperAsync(TaskComment comment, TaskCommentAttachmentDto file, TaskPersonCommand user)
        {
            TaskCommentAttachmentMapping commentAttachment = new TaskCommentAttachmentMapping();
            commentAttachment.AttachmentName = file.AttachmentDesc;
            commentAttachment.AttachmentGuid = file.AttachmentGuid;
            commentAttachment.AttachmentSize = file.AttachmentSize;
            commentAttachment.CreatedBy = JsonConvert.SerializeObject(user);
            commentAttachment.CreatedDate = System.DateTime.Now;
            commentAttachment.Comment = comment;
            return commentAttachment;
        }

        private void Create(TaskComment request)
        {
            _repository.Add(request);
        }


        private void UpdateComment(TaskComment request, List<string> delAttachments)
        {
            if (request != null)
            {
                if (request.TaskCommentAttachmentMapping.Count > 0)
                    _repository.Add(request);

                _repository.Update(request);

                if (delAttachments.Count > 0)
                    _repository.Delete(delAttachments);
            }
            else
                throw new NullReferenceException($" Comment was null");
        }

        private void PublishThroughEventBusForDeleteAttachments(IList<TaskCommentAttachmentDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForDelete(evt);
            }
        }

        private void PublishThroughEventBusForMappingAttachments(IList<TaskCommentAttachmentDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForMapping(evt);
            }
        }

        private void CheckRoles(EliteTask task)
        {
            if (this.rolesPermissions.UserRolesAndRights?.Count > 0)
            {
                RolePermissions rolePermissions = new RolePermissions();

                var roles = task.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(task.CommitteeId)) :
                    this.rolesPermissions.UserRolesAndRights.FirstOrDefault(s => s.CommitteeId == null);
                var roleId = roles != null ? roles.RoleId : (int?)null;
                var substask = task.InverseParent;

                var permissionsActions = rolePermissions.GetUserAction(_configuration, task.CreatedBy.Upper(), task.Responsible.Upper(), task.CoResponsibles != null ? task.CoResponsibles.Upper() : "", substask.Any(s => s.Responsible.Upper().Contains(pUID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, task.MeetingId.HasValue, pUID);

                if (!rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.Comment))
                {
                    throw new EliteException($"unauthorized");
                }
            }
            else { throw new EliteException($"unauthorized"); }
        }



    }
}


using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.ResilientTransaction;
using Elite.EventBus.Services;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Elite_Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Microsoft.EntityFrameworkCore.Storage;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class UpdateTopicMeetingDetailsCommandHandler : BaseCommandHandler<UpdateTopicMeetingDetailsCommand>, IRequestHandler<UpdateTopicMeetingDetailsCommand, bool>
    {
        protected readonly IMediator _mediator;
        protected readonly ITaskRepository _repository;
        protected readonly IConfiguration _configuration;

        private readonly Func<IConfiguration, IRequestContext, IMeetingTaskService> _meetingServiceFactory;
        private readonly IMeetingTaskService _meetingService;
        private readonly IList<BaseNotificationEvent> _taskNotifications;
        private readonly Func<DbConnection, IEventStoreService> _integrationEventStoreServiceFactory;
        private readonly IEventStoreService _integrationEventStoreService;

        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;

        private readonly IRequestContext _requestContext;
        private IList<LookUp> committees;
        public IList<UserRolesAndRights> UserRolesAndRights { get; private set; }
        private readonly string securedUID = string.Empty;
        private TaskPersonCommand _modifiedBy;
        private DateTime? _modifiedDate;

        public UpdateTopicMeetingDetailsCommandHandler(
           IMediator mediator,
           IConfiguration configuration,
           ITaskRepository repository,
           Func<IConfiguration, IRequestContext, IMeetingTaskService> meetingServiceFactory,
            IRequestContext requestContext,
            Func<IConfiguration, IRequestContext, IUserService> userServiceFactory,
             Func<DbConnection, IEventStoreService> integrationEventStoreServiceFactory


           )
        {
            this._requestContext = requestContext;
            this._mediator = mediator;
            this._repository = repository;
            this._meetingServiceFactory = meetingServiceFactory;
            this._configuration = configuration;
            this._meetingService = this._meetingServiceFactory(this._configuration, requestContext);
            this._taskNotifications = new List<BaseNotificationEvent>();
            this._integrationEventStoreServiceFactory = integrationEventStoreServiceFactory;
            this._integrationEventStoreService = this._integrationEventStoreServiceFactory(
                ((EliteTaskContext)this._repository.UnitOfWork).Database.GetDbConnection());
            _userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            securedUID = requestContext.DecrpUID;
        }

        public async Task<bool> Handle(UpdateTopicMeetingDetailsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _modifiedBy = JsonConvert.DeserializeObject<TaskPersonCommand>(JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID)));
                _modifiedDate = DateTime.Now;
                int result = 0;
                var taskList = await _repository.GetTaskAndSubTaskRangeByIdAsync(request.TaskIds);

                if (taskList != null)
                {
                    foreach (var task in taskList)
                    {
                        task.Action = (int)DatabaseAction.Update;
                        task.ModifiedBy = _modifiedBy != null ? JsonConvert.SerializeObject(_modifiedBy) : null;
                        task.ModifiedDate = _modifiedDate;
                        task.AgendaId = task.AgendaId;
                        task.MeetingId = task.MeetingId;
                        task.MeetingStatus = task.MeetingStatus;
                    }
                    await ResilientTransaction.New((EliteTaskContext)_repository.UnitOfWork).ExecuteAsync(async () =>
                    {
                        result = _repository.UpdateRange(taskList);
                    });
                    return result > 0 ? true : false;
                }
                else
                    throw new EliteException("Task not found");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

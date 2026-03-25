using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.ResilientTransaction;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Topic.Microservice.Application.CQRS.Commands
{
    public class TaskPublishedAndUnPublishedCommandHandler : IRequestHandler<TaskPublishedAndUnPublishedCommand, bool>
    {
        protected readonly ITaskRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;
        private readonly string securedUID = string.Empty;
        private TaskPersonCommand _modifiedBy;
        private DateTime? _modifiedDate;

        public TaskPublishedAndUnPublishedCommandHandler(
            ITaskRepository repository,
            IConfiguration configuration,
            IRequestContext requestContext,
            Func<IConfiguration, IRequestContext, IUserService> userServiceFactory
            )
        {
            this._configuration = configuration;
            this._repository = repository;
            _userServiceFactory = userServiceFactory;
            _userService = _userServiceFactory(configuration, requestContext);
            securedUID = requestContext.DecrpUID;
        }

        public async Task<bool> Handle(TaskPublishedAndUnPublishedCommand request, CancellationToken cancellationToken)
        {
			try
			{
				return await SaveTask(request);
			}
			catch (Exception)
			{
				throw;
			}
        }

        private async Task<bool> SaveTask(TaskPublishedAndUnPublishedCommand request)
        {
            _modifiedBy = JsonConvert.DeserializeObject<TaskPersonCommand>(JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID)));
            _modifiedDate = DateTime.Now;
            bool isSaved = false;
            var tasks = new List<EliteTask>();

            if (request.Tasks?.Count > 0)
            {
                foreach (var task in request.Tasks)
                {
                    var taskObj = await _repository.GetByIdAsync(task.Id);
                    if (taskObj != null)
                    {
                        taskObj.MeetingId = request.Id;
                        taskObj.MeetingStatus = request.MeetingStatus;
                        taskObj.ModifiedBy = JsonConvert.SerializeObject(_modifiedBy);
                        taskObj.ModifiedDate = _modifiedDate.HasValue ? _modifiedDate.Value : DateTime.Now;
                    }

                    if (taskObj != null)
                    {
                        tasks.Add(taskObj);
                    }
                }

                if (tasks?.Count > 0)
                {
                    isSaved = _repository.UpdateRange(tasks) != 0;
                }
            }
            return isSaved;
        }
    }
}

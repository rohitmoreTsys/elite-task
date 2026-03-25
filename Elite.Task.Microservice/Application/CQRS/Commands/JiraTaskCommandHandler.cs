using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
    public class JiraTaskCommandHandler : BaseCommandHandler<JiraTaskCommand>, IRequestHandler<JiraTaskCommand, long>
    {
        protected readonly ITaskRepository _repository;
        protected readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IJiraService _jiraTaskService;
        private readonly Func<IConfiguration, IRequestContext, IJiraService> _jiraServiceFactory;
        public JiraTaskCommandHandler(ITaskRepository repository,
            IConfiguration configuration,
            IRequestContext requestContext,
            Func<IConfiguration, IRequestContext, IUserService> userServiceFactory,
                 Func<IConfiguration, IRequestContext, IJiraService> jiraServiceFactory)
        {
            this._repository = repository;
            this._configuration = configuration;
            this._userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            this._jiraServiceFactory = jiraServiceFactory;
            this._jiraTaskService = _jiraServiceFactory(configuration, requestContext);

        }

        public async Task<long> Handle(JiraTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _repository.GetByIdAsync(request.Id);
            if (task.JiraTicketInfo != null)
            {
                var result = await _jiraTaskService.GetTaskFromJira(JsonConvert.DeserializeObject<JiraTicketInfo>(task.JiraTicketInfo).JiraIssueKey);
                // Request for JIRA Task
                if (result.fields.assignee != null && !string.IsNullOrEmpty(result.fields.assignee.name))
                {
                    // Jira Responsibles are fetching from GSEP JIRA. So DisplayName
                    //mismatch is there + If this User(available in JIRA), but
                    //not in eLite create the User, return the  Actual Display according to LDAP.
                    TaskPersonCommand jiraResponsible = new TaskPersonCommand(result.fields.assignee.name, string.Empty);
                    var userInfo = await this._userService.ValidateAndPostUsers(jiraResponsible);

                    task.Responsible = JsonConvert.SerializeObject(new QueriesPersonDto() { DisplayName = userInfo.DisplayName, Uid = userInfo.Uid });
                    task.Title = result.fields.summary;
                    task.Description = result.fields.description;
                    task.DueDate = DateTime.Parse(result.fields.duedate);
                    task.Status = (int)Enum.Parse(typeof(Elite_Task.Microservice.CommonLib.TaskStatus), result.fields.status.name.Replace(" ", ""));
                    await _repository.UnitOfWork.SaveEntitiesAsync();
                }
            }
            return task.Id;
        }
    }
}

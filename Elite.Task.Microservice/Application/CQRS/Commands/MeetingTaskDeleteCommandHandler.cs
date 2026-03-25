using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.ResilientTransaction;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.EventBus.Services;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Elite.Task.Microservice.CommonLib;
using Elite.Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class MeetingTaskDeleteCommandHandler : BaseCommandHandler<MeetingTaskDeleteCommand>, IRequestHandler<MeetingTaskDeleteCommand, bool>
    {
        protected readonly IMediator _mediator;
        protected readonly ITaskRepository _repository;
        protected readonly IRepositoryEventStore _repositoryEventStore;
        protected readonly IConfiguration _configuration;

        private readonly Func<IConfiguration, IRequestContext, IMeetingTaskService> _meetingServiceFactory;
        private readonly IMeetingTaskService _meetingService;
        private readonly IList<BaseNotificationEvent> _taskNotifications;
        private readonly Func<DbConnection, IEventStoreService> _integrationEventStoreServiceFactory;
        private readonly IEventStoreService _integrationEventStoreService;

        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, ITopicServices> _topicServiceFactory;
        private readonly ITopicServices _topicService;

        private readonly IRequestContext _requestContext;
        private IList<CommitteeDetail> committees;
        public IList<UserRolesAndRights> UserRolesAndRights { get; private set; }
        private readonly IJiraService _jiraTaskService;
        private readonly Func<IConfiguration, IRequestContext, IJiraService> _jiraServiceFactory;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;
        private string UID;
        private readonly string securedUID = string.Empty;
        private TaskPersonCommand _modifiedBy;
        private DateTime? _modifiedDate;

        public MeetingTaskDeleteCommandHandler(
           IMediator mediator,
           IConfiguration configuration,
           ITaskRepository repository,
           Func<IConfiguration, IRequestContext, IMeetingTaskService> meetingServiceFactory,
           Func<IConfiguration, IRequestContext, ITopicServices> topicServiceFactory,
           IRequestContext requestContext,
           Func<IConfiguration, IRequestContext, IUserService> userServiceFactory,
           Func<DbConnection, IEventStoreService> integrationEventStoreServiceFactory,
           IRepositoryEventStore repositoryEventStore,
           Func<IConfiguration, IRequestContext, IJiraService> jiraServiceFactory

           )
        {
            this._requestContext = requestContext;
            this._mediator = mediator;
            this._repository = repository;
            this._meetingServiceFactory = meetingServiceFactory;
            this._configuration = configuration;
            this._meetingService = this._meetingServiceFactory(this._configuration, requestContext);
            this._topicServiceFactory = topicServiceFactory;
            this._topicService = topicServiceFactory(this._configuration, requestContext);
            this._taskNotifications = new List<BaseNotificationEvent>();
            this._integrationEventStoreServiceFactory = integrationEventStoreServiceFactory;
            this._integrationEventStoreService = this._integrationEventStoreServiceFactory(
                ((EliteTaskContext)this._repository.UnitOfWork).Database.GetDbConnection());
            _userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            this._repositoryEventStore = repositoryEventStore;
            this._jiraServiceFactory = jiraServiceFactory;
            this._jiraTaskService = _jiraServiceFactory(configuration, requestContext);
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            this.UID = requestContext.IsDeputy ? requestContext.DeputyUID != null ? requestContext.DeputyUID.Upper() : string.Empty
                                                : requestContext.UID != null ? requestContext.UID.ToUpper() : string.Empty;
            InitializeUsers();
            securedUID = requestContext.DecrpUID;
        }

        public async Task<bool> Handle(MeetingTaskDeleteCommand request, CancellationToken cancellationToken)
        {
            return await DeleteTask(request);
        }

        private async Task<bool> DeleteTask(MeetingTaskDeleteCommand request)
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
                        CheckRoles(task);

                        task.IsActive = false;
                        task.Action = (int)DatabaseAction.Delete;
                        task.ModifiedBy = _modifiedBy != null ? JsonConvert.SerializeObject(_modifiedBy) : null;
                        task.ModifiedDate = _modifiedDate;
                        task.TaskGuid = Guid.NewGuid().ToString();

                        if (task.TaskComment != null)
                            task.TaskComment.ToList().ForEach(x => { x.IsActive = false; x.Action = (int)DatabaseAction.Delete; x.ModifiedBy = JsonConvert.SerializeObject(_modifiedBy); x.ModifiedDate = _modifiedDate; });

                        if (task.InverseParent != null)
                            task.InverseParent.ToList().ForEach(x =>
                            {
                                x.IsActive = false; x.Action = (int)DatabaseAction.Delete;
                                x.ModifiedBy = (_modifiedBy != null ? JsonConvert.SerializeObject(_modifiedBy) : null); x.ModifiedDate = _modifiedDate;
                                x.TaskComment.ToList().ForEach(y =>
                                {
                                    x.IsActive = false; x.Action = (int)DatabaseAction.Delete;
                                    x.ModifiedBy = (_modifiedBy != null ? JsonConvert.SerializeObject(_modifiedBy) : null); x.ModifiedDate = _modifiedDate;
                                });
                            });

                        //Check if a task creation email is already sent. 
                        //If it's sent then a notification eventstore  object would be retrieved from the db.
                        var notifications = await _repositoryEventStore.GetByTaskId(task.Id);
                        if (notifications.Any())
                            AddNotification(task, _modifiedBy, request.IsDemoUser);



                        if (task.IsPublishedToJira.HasValue && task.IsPublishedToJira.Value)
                        {
                            await SetTaskStatusToDeleteInJira(task);
                        }

                    }
                    if (taskList[0] != null)
                    {
                        StringBuilder addinfo = new StringBuilder();
                        EliteTask t1 = new EliteTask();
                        t1 = taskList[0];
                        long agendaid = t1.AgendaId != null ? (long)t1.AgendaId : 0;
                        addinfo.Append("<p>");
                        if (!string.IsNullOrEmpty(t1.Title))
                            addinfo.Append("<i>Title</i>: " + t1.Title.Trim() + "; &nbsp");
                        if (t1.DueDate != null)
                            addinfo.Append("<i>Due Date</i>: " + DateTime.Parse(t1.DueDate.ToString()).ToString("MM/dd/yyyy") + "; &nbsp");
                        if (_modifiedBy != null)
                            addinfo.Append("<i>Modified By</i>: " + _modifiedBy.DisplayName + "; &nbsp");
                        if (t1.ModifiedDate != null)
                            addinfo.Append("<i>Modified Date</i>: " + DateTime.Parse(t1.ModifiedDate.ToString()).ToString("MM/dd/yyyy") + "; &nbsp");
                        if (t1.Responsible != null)
                            addinfo.Append("<i>Responsible</i>: " + JsonConvert.DeserializeObject<TaskPersonCommand>(t1.Responsible).DisplayName + "; &nbsp");
                        addinfo.Append("</p>");

                        if (agendaid > 0)
                        {
                            await this._topicService.PublishTopicHistoryAsync(new TopicHistoryEvent()
                            {
                                CategoryType = TopicHistoryStatus.TaskDeleted,
                                Comments = t1.Description,
                                GroupId = GroupType.TASK,
                                ReferenceId = t1.Id,
                                TopicId = await _meetingService.GetTopicId(agendaid),
                                CreatedBy = _modifiedBy,
                                AdditionalInfo = addinfo.ToString()
                            });
                        }
                    }
                    await ResilientTransaction.New((EliteTaskContext)_repository.UnitOfWork).ExecuteAsync(async () =>
                    {
                        result = _repository.UpdateRange(taskList);
                        if (!request.IsDeleteFromMeeting)
                        {
                            MeetingMinuteTaskDeleteEvent evt = new MeetingMinuteTaskDeleteEvent();
                            evt.TaskIds = taskList.Select(c => c.Id).ToList();
                            evt.ModifiedBy = _modifiedBy;
                            evt.ModifiedDate = _modifiedDate;
                            await ProcessEventStore(taskList[0]);
                            await _meetingService.PublishDeleteMeetingTaskThroughEventBusAsync(evt);

                        }
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


        //Update the elite task's updated information in jira.
        private async Task SetTaskStatusToDeleteInJira(EliteTask task)
        {
            var jiraTicket = JsonConvert.DeserializeObject<JiraTicketInfo>(task.JiraTicketInfo);
            await this._jiraTaskService.SetTaskStatusToDeleteInJira(jiraTicket.JiraIssueKey);
        }

        /// <summary>
        /// Add Notifications
        /// </summary>
        ///  Divya Kudalkar on 28-08-2019
        /// <param name="task"></param>
        /// <returns></returns>
        private void InitializeUsers()
        {
            Task.Run(async () =>
            {
                this.committees = await this._userService.GetCommitees();

            }).Wait();
        }
        private string GetCommitteeName(long? committeeId)
        {
            if (committeeId.HasValue)
            {
                var committee = this.committees.FirstOrDefault(p => p.CommitteeId.Equals(committeeId));
                if (committee != null)
                    return committee.CommitteeName;
                else
                    return string.Empty;
            }
            else return string.Empty;
        }

        private string GetCommitteePoolIdEmailId(long? committeeId)
        {
            if (committeeId.HasValue)
            {
                var committee = this.committees.FirstOrDefault(p => p.CommitteeId.Equals(committeeId));
                if (committee != null)
                    return committee.PoolIdEmailId;
                else
                    return string.Empty;
            }
            else return string.Empty;
        }
        private string GetCommitteePoolIdName(long? committeeId)
        {
            if (committeeId.HasValue)
            {
                var committee = this.committees.FirstOrDefault(p => p.CommitteeId.Equals(committeeId));
                if (committee != null)
                    return committee.PoolIdName;
                else
                    return string.Empty;
            }
            else return string.Empty;
        }

        private void GetUserRolesAndRights(string uid)
        {
            Task.Run(async () =>
            {
                this.UserRolesAndRights = await this._userService.GetUserRolesAndRights(uid);

            }).Wait();
        }
        private string GetUserRole(string uid, long? committeeId)
        {

            if (uid != null)
            {
                GetUserRolesAndRights(uid);
                var role = this.UserRolesAndRights.FirstOrDefault(p => p.UID == uid.ToUpper() && p.CommitteeId == committeeId);
                if (role != null)
                    return role.RoleName;
                else
                    return null;

            }
            else
                return null;
        }
        private void AddNotification(EliteTask task, TaskPersonCommand user, bool demoUser)
        {
            string committeeName = GetCommitteeName(task.CommitteeId);
            string userRole = GetUserRole(user.Uid, task.CommitteeId);
            string committeePoolIdEmail = GetCommitteePoolIdEmailId(task.CommitteeId);
            string committeePoolIdName = GetCommitteePoolIdName(task.CommitteeId);
            string description = task.Description != null ? task.Description : string.Empty;
            var respList = !string.IsNullOrEmpty(task.Responsible) ? JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible)
                            : new TaskPersonCommand("", "");
            var coRespList = !string.IsNullOrEmpty(task.CoResponsibles) ? JsonConvert.DeserializeObject<List<TaskPersonCommand>>(task.CoResponsibles)
                            : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);

            if (task != null)
            {
                var taskResponsible = JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible);
                AddTaskEmailNotification(task, NotificationActionType.TASK_TO_DELETE, user, taskResponsible, task.Title, null, null, task.DueDate,
                   task.CommitteeId, committeeName, userRole, demoUser, committeePoolIdEmail, committeePoolIdName, false, taskPeople, _modifiedBy);

                if (task.CoResponsibles != null)
                {
                    var coResponsibles = JsonConvert.DeserializeObject<List<TaskGroupCommand>>(task.CoResponsibles);
                    var resp = JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible);
                    foreach (var responsible in coResponsibles)
                    {
                        var coresponsible = new TaskPersonCommand(responsible.Uid, responsible.DisplayName);
                        if (responsible.users != null)
                        {
                            foreach (var userResp in responsible.users)
                            {
                                if (userResp.Uid.ToUpper() != resp.Uid.ToUpper())
                                    AddTaskEmailNotification(task, NotificationActionType.TASK_TO_DELETE, userResp, userResp, task.Title, task.Id,
                                        description, task.DueDate, task.CommitteeId, committeeName, userRole, demoUser, committeePoolIdEmail, committeePoolIdName, false, taskPeople, _modifiedBy);
                            }
                        }
                        else
                        {
                            if (coresponsible.Uid.ToUpper() != resp.Uid.ToUpper())
                                AddTaskEmailNotification(task, NotificationActionType.TASK_TO_DELETE, user, coresponsible, task.Title, task.Id,
                                    description, task.DueDate, task.CommitteeId, committeeName, userRole, demoUser, committeePoolIdEmail, committeePoolIdName, false, taskPeople, _modifiedBy);
                        }
                    }
                }
            }
            if (task.InverseParent?.Count > 0 || (user != null && task != null))
            {
                foreach (var inverseParent in task.InverseParent)
                {
                    var responsible = JsonConvert.DeserializeObject<TaskPersonCommand>(inverseParent.Responsible);
                    var subTaskPeople = new TaskPeople(responsible, new List<TaskPersonCommand>());
                    AddTaskEmailNotification(task, NotificationActionType.SUBTASK_TO_DELETE, user, responsible, inverseParent.Title, null, null,
                        inverseParent.DueDate, task.CommitteeId, committeeName, userRole, demoUser, committeePoolIdEmail, committeePoolIdName, true, subTaskPeople, _modifiedBy);
                }
            }
        }

        /// <summary>
        /// Process the Notification
        /// </summary>
        ///  Divya Kudalkar on 28-08-2019
        /// <param name="task"></param>
        /// <returns></returns>

        private async Task ProcessEventStore(EliteTask task)
        {
            var eventStores = new List<Elite.EventBus.EventStore.EliteEventStoreDto>();
            //Process notification
            this._taskNotifications.ToList().ForEach(p =>
            {
                var _event = p as EmailNotificationEvent;

                long id = 0;
                if (_event.NotificationID.ToString().Equals(task.TaskGuid.ToString()))
                {
                    id = task.Id;
                }
                else if (task.InverseParent?.Count > 0)
                {
                    var data = task.InverseParent.FirstOrDefault(x => x.TaskGuid.ToString().Equals(_event.NotificationID.ToString()));

                    if (data != null)
                    {
                        id = data.Id;
                    }
                }
                eventStores.Add(new Elite.EventBus.EventStore.EliteEventStoreDto()
                {
                    ActionType = (int)p.ActionType,
                    JsonMessage = JsonConvert.SerializeObject(p.Message),
                    CreatedDate = DateTime.Now,
                    NotificationType = (int)NotificationType.Email,
                    IsProcessed = true,
                    GroupId = (int)GroupType.TASK,
                    Sourcetypeid = id
                });
            });

            if (eventStores?.Count > 0)
            {
                await this._integrationEventStoreService.SaveEventAsync(eventStores,
                    ((EliteTaskContext)_repository.UnitOfWork).Database.CurrentTransaction.GetDbTransaction());
            }
        }

        /// <summary>
        /// Method to add task notification
        /// </summary>
        private void AddTaskEmailNotification(EliteTask task, NotificationActionType actionType, TaskPersonCommand createdBy, TaskPersonCommand responsible,
                    string taskTitle, long? taskId, string description, DateTime? dueDate, long? committeeId, string committeeName, string role, bool isDemoUser, string poolIdEmailId,
                    string poolIdName, bool isSubTask = false, TaskPeople taskPeople = null, TaskPersonCommand modifiedBy = null)
        {
            // taskResp and taskCoResp are used to display the responsible and co-responsible persons in the task template
            var taskResp = taskPeople?.Responsible?.DisplayName ?? string.Empty;

            var taskCoResp = string.Join("<br/>", (taskPeople?.CoResponsibles?
                            .Where(c => !string.IsNullOrWhiteSpace(c?.DisplayName))
                            .Select(c => c.DisplayName)
                            ) ?? Enumerable.Empty<string>());
            this._taskNotifications.Add(new EmailNotificationEvent
            {
                NotificationID = new Guid(task.TaskGuid),
                ActionType = actionType,
                Message = new TaskEvent
                {
                    CreatedBy = createdBy,
                    Responsible = responsible,
                    TaskTitle = taskTitle,
                    TaskId = taskId,
                    Description = description,
                    DueDate = dueDate,
                    CommitteeId = committeeId,
                    CommitteeName = committeeName,
                    Role = role,
                    IsDemoUser = isDemoUser,
                    PoolIdEmailId = poolIdEmailId,
                    PoolIdName = poolIdName,
                    TaskResponsible = taskResp,
                    TaskCoResponsible = taskCoResp,
                    ModifiedBy = modifiedBy
                },
                GroupID = GroupType.TASK
            });
        }

        private void CheckRoles(EliteTask task)
        {
            if (this.rolesPermissions.UserRolesAndRights?.Count > 0)
            {
                RolePermissions rolePermissions = new RolePermissions();

                var roles = task.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(task.CommitteeId)) : null;
                var roleId = roles != null ? roles.RoleId : (int?)null;
                var substask = task.InverseParent;

                var permissionsActions = rolePermissions.GetUserAction(_configuration, task.CreatedBy.Upper(), task.Responsible.Upper(), task.CoResponsibles != null ? task.CoResponsibles.Upper() : "", substask.Any(s => s.Responsible.Upper().Contains(UID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, task.MeetingId.HasValue, UID);

                if (!rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.DeleteTask))
                {
                    throw new EliteException($"unauthorized");
                }
            }
            else
            {
                throw new EliteException($"unauthorized");
            }
        }

    }
}

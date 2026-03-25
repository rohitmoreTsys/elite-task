using Elite.Common.Utilities.Attachment.Delete.MapOrphans;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HtmlAgilityPack;
using Elite.Common.Utilities.JiraEntities;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.ResilientTransaction;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.EventBus.Services;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.CommonLib;
using Elite.Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Application.CQRS.Commands.CommmandsDto;
using Elite_Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Libuv.Internal.Networking;
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
using MeetingInfo = Elite.Common.Utilities.CommonType.MeetingInfo;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class TaskCommandHandler : BaseCommandHandler<TaskCommand>, IRequestHandler<TaskCommand, long>
    {

        long requestId;
        List<long> subTaskIds = new List<long>();
        protected readonly IMediator _mediator;
        protected readonly ITaskRepository _repository;
        protected readonly ICommentRepository _commentRepository;
        protected readonly IConfiguration _configuration;
        private readonly Func<IConfiguration, IAttachmentService> _attachmentServiceFactory;
        private readonly IAttachmentService _attachmentService;
        private readonly Func<IConfiguration, IRequestContext, IMeetingTaskService> _meetingServiceFactory;
        private readonly Func<DbConnection, IEventStoreService> _integrationEventStoreServiceFactory;
        private readonly IEventStoreService _integrationEventStoreService;
        private readonly IMeetingTaskService _meetingService;
        private readonly string UID;
        private readonly IList<BaseNotificationEvent> _taskNotifications;
        private readonly IList<ProtocolTask> _protocolTaskList;
        private bool IsAgendaTopic = false;
        private readonly Func<IConfiguration, IRequestContext, ITopicServices> _topicServiceFactory;
        private readonly ITopicServices _topicService;
        private readonly IUserService _userService;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IJiraService _jiraTaskService;
        private readonly Func<IConfiguration, IRequestContext, IJiraService> _jiraServiceFactory;
        private IList<CommitteeDetail> committees;
        public IList<UserRolesAndRights> UserRolesAndRights { get; private set; }
        public IList<long> delSubTaskIds { get; private set; }
        public IList<long> updateSubTaskIds { get; private set; }
        private bool IsReminder = false;
        private long notifystatus = 0;
        public MeetingInfo meetingInfo;
        public IEnumerable<string> AttachmentsToUpdate { get; set; }
        private bool isRemiderForSubTask = false;
        private string committeeName;
        private string committeePoolIdEmail;
        private string committeePoolIdName;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;
        private string pUID;
        private readonly string securedUID = string.Empty;
        private TaskPersonCommand _createdBy;
        private TaskPersonCommand _modifiedBy;
        private DateTime? _createdDate;
        private DateTime? _modifiedDate;
        private bool isEliteClassic;

        public TaskCommandHandler(
            IMediator mediator,
            ITaskRepository repository,
            IConfiguration configuration,
            Func<IConfiguration, IAttachmentService> attachmentServiceFactory,
            Func<IConfiguration, IRequestContext, IMeetingTaskService> meetingServiceFactory,
            IRequestContext requestContext,
            Func<DbConnection, IEventStoreService> integrationEventStoreServiceFactory,
            Func<IConfiguration, IRequestContext, ITopicServices> topicServiceFactory,
            Func<IConfiguration, IRequestContext, IUserService> userServiceFactory,
                 Func<IConfiguration, IRequestContext, IJiraService> jiraServiceFactory,
                 ICommentRepository commentRepository
            )
        {
            this._mediator = mediator;
            this._repository = repository;
            this._configuration = configuration;
            this._attachmentServiceFactory = attachmentServiceFactory;
            this._attachmentService = this._attachmentServiceFactory(this._configuration);
            this._meetingServiceFactory = meetingServiceFactory;
            this._meetingService = this._meetingServiceFactory(this._configuration, requestContext);
            this._taskNotifications = new List<BaseNotificationEvent>();
            this.UID = requestContext.UID.ToUpper();
            this._integrationEventStoreServiceFactory = integrationEventStoreServiceFactory;
            this._integrationEventStoreService = this._integrationEventStoreServiceFactory(((EliteTaskContext)this._repository.UnitOfWork).Database.GetDbConnection());
            _protocolTaskList = new List<ProtocolTask>();
            this._topicServiceFactory = topicServiceFactory;
            this._topicService = topicServiceFactory(this._configuration, requestContext);
            _userServiceFactory = userServiceFactory;
            this._userService = _userServiceFactory(configuration, requestContext);
            this._jiraServiceFactory = jiraServiceFactory;
            this._jiraTaskService = _jiraServiceFactory(configuration, requestContext);
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            this.pUID = requestContext.IsDeputy ? requestContext.DeputyUID != null ? requestContext.DeputyUID.Upper() : string.Empty
                                                : requestContext.UID != null ? requestContext.UID.ToUpper() : string.Empty;
            InitializeUsers();
            securedUID = requestContext.DecrpUID;
            _commentRepository = commentRepository;
            isEliteClassic = _configuration.GetSection("isEliteClassic").Get<bool>();
        }

        public async Task<long> Handle(TaskCommand request, CancellationToken cancellationToken)
        {
            try
            {
                return await SaveTaskAndAttachment(request);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<long> SaveTaskAndAttachment(TaskCommand request)
        {
            bool isStatusUpdate = false;
            EliteTask task = null;
            delSubTaskIds = new List<long>();
            updateSubTaskIds = new List<long>();
            task = await _repository.GetByIdAsync(request.Id);
            if (task != null)
            {
                _createdBy = JsonConvert.DeserializeObject<TaskPersonCommand>(task.CreatedBy);
                _createdDate = task.CreatedDate;
                if (request.Status == (int)CommonLib.TaskStatus.Completed)
                {
                    task.CompletionDate = DateTime.Now;
                }
                // Assign topic title values to each task
                task.topicTitleInEnglish = request.topicTitleInEnglish ?? string.Empty;
                task.topicTitleInGerman = request.topicTitleInGerman ?? string.Empty;
            }
            else
            {
                _createdBy = JsonConvert.DeserializeObject<TaskPersonCommand>(JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID)));
                _createdDate = DateTime.Now;
            }
            _modifiedBy = JsonConvert.DeserializeObject<TaskPersonCommand>(JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID)));
            _modifiedDate = DateTime.Now;

            committeeName = GetCommitteeName(request.CommitteeId);
            committeePoolIdEmail = GetCommitteePoolIdEmail(request.CommitteeId);
            committeePoolIdName = GetCommitteePoolIdName(request.CommitteeId);

            if (task is null)
                task = new EliteTask();
            else
                isStatusUpdate = task.Status != request.Status;

            if (request.MeetingID != null && meetingInfo == null)
            {
                meetingInfo = new MeetingInfo();
                //service call for getting the meeting info
                meetingInfo = await GetTaskMeetingInfo((long)request.MeetingID, request.Id);
                if (task.AgendaId.HasValue)
                {
                    try
                    {
                        var agendaDetailsById = await _meetingService.GetAgendaTitle(task.AgendaId ?? 0);
                        if (!string.IsNullOrEmpty(agendaDetailsById))
                        {
                            var agendaDetails = JsonConvert.DeserializeObject<AgendaTitleDetails>(agendaDetailsById);
                            if (agendaDetails != null)
                            {
                                task.topicTitleInEnglish = agendaDetails.Title;
                                task.topicTitleInGerman = agendaDetails.TitleInGerman;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception occurred while getting agendatitle details: {ex.Message}");
                    }
                }
            }
            // Request for JIRA Task
            if (request.JiraTicketInfo != null && request.Responsible != null)
            {
                // Jira Responsibles are fetching from GSEP JIRA. So DisplayName
                //mismatch is there + If this User(available in JIRA), but
                //not in eLite create the User, return the  Actual Display according to LDAP.
                TaskPersonCommand jiraResponsible = new TaskPersonCommand(request.Responsible.Uid, request.Responsible.DisplayName);
                var userInfo = await this._userService.ValidateAndPostUsers(jiraResponsible);
                //Assing the LDAP UID/Display Name or DB UID and Display Name
                //based on User newly created or not.
                request.Responsible.Uid = userInfo.Uid;
                request.Responsible.DisplayName = userInfo.DisplayName;
            }
            //Create and Update Task
            //used for the update flag
            requestId = request.Id;
            // Check if the main task needs to be created or updated based on request and current task state(used for avoiding unnecessary mail triggering)
            bool shouldUpdateMainTask = ShouldUpdateMainTask(request, task);
            if (shouldUpdateMainTask)
            {
                if (IsUpdate(request.Id))
                    await Update(request, task);
                else
                    Create(request, task);
            }

            //Create and Update SubTask
            await CreateandUpdateSubTaskMapper(request.SubTask, task);

            CheckRoles(task, request.SubTask);

            await ResilientTransaction.New((EliteTaskContext)_repository.UnitOfWork).ExecuteAsync(async () =>
            {
                updateSubTaskIds = request.SubTask.Where(x => x.Id > 0 && x.IsDeleted != true).Select(x => x.Id).ToList();
                await _repository.UnitOfWork.SaveEntitiesAsync();
                //notification should trigger when IsFinalMinutes is always true and either it is reminder or notifyUsers
                if (request.IsFinalMinutesTasks == true || request.IsReminder == true)
                {
                    await ProcessEventStore(task);
                }
                //publish to Meeting service to update Task status in Agenda task            
                await PublishMeetingTask(request, task);
                await PublishMeetingAgenda(request, task);

            });
            //Save Task and SubTask    

            await ProcessProtocolTask(task);

            //publishing to kafka ---> to delete the attachment from Attachment service
            PublishThroughEventBusForDeleteAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.DeleteAttachment));

            //publishing to kafka ---> to set IsOrphen to false for mapping topic attachment
            PublishThroughEventBusForMappingAttachments(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.MappOrphanAttachment));

            //Publish the information to jira. 
            //Not awaiting this call since there is no control on the jira api call.
            //If this fails, then it will be published again when the task is edited in elite.
            if (request.IsDeleted.HasValue && request.IsDeleted.Value == false)
                await PublishInformationToJira(request, task, isStatusUpdate);

            return task.Id;
        }

        private async Task PublishInformationToJira(TaskCommand request, EliteTask task, bool isStatusUpdate)
        {
            if (request.JiraTicketInfo != null &&
                string.IsNullOrEmpty(request.JiraTicketInfo.JiraIssueKey) &&
                string.IsNullOrEmpty(request.JiraTicketInfo.JiraIssueId) &&
                string.IsNullOrEmpty(request.JiraTicketInfo.JiraProjectName) == false &&
                string.IsNullOrEmpty(request.JiraTicketInfo.JiraProjectKey) == false)
            {
                await PublishTicketToJira(task, request);
            }
            if (IsUpdate(request.Id) &&
                isPublishedToJira(request.JiraTicketInfo))
            {
                await UpdateTicketInJira(task, request, isStatusUpdate);
            }

        }


        public bool isPublishedToJira(JiraTicketInfo jiraTicketInfo)
        {
            return jiraTicketInfo != null &&
                string.IsNullOrEmpty(jiraTicketInfo.JiraIssueKey) == false &&
                string.IsNullOrEmpty(jiraTicketInfo.JiraIssueId) == false &&
                string.IsNullOrEmpty(jiraTicketInfo.JiraProjectName) == false &&
                string.IsNullOrEmpty(jiraTicketInfo.JiraProjectKey) == false;
        }

        #region Jira Related Functions

        //Publish the ticket to jira and wait for the response.
        //And update the task information with task url and save the changes in db.
        //To do : Move jira related functions to a separate command handler class.
        private async Task PublishTicketToJira(EliteTask task, TaskCommand request)
        {
            var jiraTicket = JsonConvert.DeserializeObject<JiraTicketInfo>(task.JiraTicketInfo);
            var data = new JiraTaskDto()
            {
                ProjectKey = jiraTicket.JiraProjectKey,
                Summary = task.Title,
                Description = HtmlAgilityUtility.ConvertHTMLToString(task.Description),
                Reporter = task.CreatedBy,
                Assignee = task.Responsible,
                DueDate = task.DueDate.Value.ToString("yyyy-MM-dd"),
                Attachments = task.TaskAttachmentMapping.Select(x => x.AttachmentGuid)
            };

            if (task.Status != (int)CommonLib.TaskStatus.Assigned)
                data.Status = Enum.GetName(typeof(CommonLib.TaskStatus), task.Status);

            //Save ticket and wait for response to get issue id and issue key.
            var jiraTicketResponse = await this._jiraTaskService.CreateTaskInJira(JsonConvert.SerializeObject(data));

            //Updating the elite task object's JiraticketInfo property with jira id and key.
            jiraTicket.JiraIssueId = jiraTicketResponse.id;
            jiraTicket.JiraIssueKey = jiraTicketResponse.key;
            task.JiraTicketInfo = JsonConvert.SerializeObject(jiraTicket);
            task.IsPublishedToJira = true;
            this._repository.Update(task);
            await this._repository.UnitOfWork.SaveChangesAsync();
        }

        //Update the elite task's updated information in jira.
        private async Task UpdateTicketInJira(EliteTask task, TaskCommand request, bool isStatusUpdate)
        {
            var jiraTicket = JsonConvert.DeserializeObject<JiraTicketInfo>(task.JiraTicketInfo);
            var data = new JiraTaskDto()
            {
                IssueKey = jiraTicket.JiraIssueKey,
                ProjectKey = jiraTicket.JiraProjectKey,
                Summary = task.Title,
                Description = HtmlAgilityUtility.ConvertHTMLToString(task.Description),
                Reporter = task.CreatedBy,
                Assignee = task.Responsible,
                DueDate = task.DueDate.Value.ToString("yyyy-MM-dd"),
                Attachments = task.TaskAttachmentMapping.Select(x => x.AttachmentGuid)
            };
            await UpdateStatus(task, isStatusUpdate, data);

            await this._jiraTaskService.UpdateTaskInJira(JsonConvert.SerializeObject(data));
        }

        private async Task UpdateStatus(EliteTask task, bool isStatusUpdate, JiraTaskDto data)
        {
            if (isStatusUpdate)
            {
                var currentStatus = await _jiraTaskService.GetTaskFromJira(data.IssueKey);
                var statusInJira = currentStatus.fields.status.name.Replace(" ", "");


                //If the task is already updated to completed in Jira
                //Then save this in eLite.
                if (statusInJira == Enum.GetName(typeof(CommonLib.TaskStatus), CommonLib.TaskStatus.Completed))
                {
                    task.Status = (int)Enum.Parse(typeof(CommonLib.TaskStatus), statusInJira);
                    this._repository.Update(task);
                    await this._repository.UnitOfWork.SaveChangesAsync();
                }
                //Save status only if it is different in jira and eLite.
                else if (statusInJira != Enum.GetName(typeof(CommonLib.TaskStatus), task.Status))
                {
                    data.Status = Enum.GetName(typeof(CommonLib.TaskStatus), task.Status);
                }
            }
        }

        #endregion

        private async Task<EliteTask> SaveSubTask(TaskCommand request, EliteTask task, bool isSubTask)
        {
            EliteTask subTask = null;
            if (request != null)
                subTaskIds.Add(request.Id);

            subTask = await _repository.GetByIdAsync(request.Id);

            if (subTask is null)
                subTask = new EliteTask();


            if (task.MeetingId != null && meetingInfo == null)
            {
                //checking for mail sending param
                meetingInfo = new MeetingInfo();
                //service call for getting the meeting info
                meetingInfo = await GetTaskMeetingInfo((long)task.MeetingId, task.Id);
                //checking param for filling data
            }

            // Assign topic title values to each task
            subTask.topicTitleInEnglish = task.topicTitleInEnglish ?? string.Empty;
            subTask.topicTitleInGerman = task.topicTitleInGerman ?? string.Empty;

            if (IsUpdate(request.Id))
                await Update(request, subTask, task, isSubTask);
            else
                Create(request, subTask, task);

            return subTask;
        }

        private void Create(TaskCommand request, EliteTask task, EliteTask parentTask = null)
        {
            if (parentTask is null)
                _repository.Add(CreateMapper(task, request));
            else
            {
                _repository.Add(CreateMapper(task, request, parentTask));
            }
        }

        private async Task Update(TaskCommand request, EliteTask task, EliteTask parentTask = null, bool isSubTask = false)
        {

            if (task.MeetingId != null && meetingInfo == null)
            {
                meetingInfo = new MeetingInfo();
                //service call for getting the meeting info
                meetingInfo = await GetTaskMeetingInfo((long)task.MeetingId, task.Id);
            }

            if (task != null)
            {
                //Create task entity from request
                if (parentTask is null)
                    CreateMapper(task, request);
                else
                    //Create sub task entity from request                    
                    CreateMapper(task, request, parentTask);


                //Update and delete sub Task
                if (request.IsDeleted.HasValue && request.IsDeleted.Value)
                {
                    if (isSubTask)
                        delSubTaskIds.Add(task.Id);
                    if (request.Id > 0)
                    {
                        var subtaskComments = await _repository.GetComments(request.Id);
                        if (subtaskComments?.Count > 0)
                        {
                            subtaskComments.ToList().ForEach(x => _repository.DeleteComments(x));
                        }
                    }
                    _repository.DeleteTask(task);
                    if (request.IsFinalMinutesTasks == true)
                    {
                        await AddDeleteNotification(task, request, isSubTask);
                    }

                    //Changing the status of the jira ticket to deleted.
                    if (task.IsPublishedToJira.HasValue && task.IsPublishedToJira.Value)
                    {
                        var jiraTicket = JsonConvert.DeserializeObject<JiraTicketInfo>(task.JiraTicketInfo);
                        await this._jiraTaskService.SetTaskStatusToDeleteInJira(jiraTicket.JiraIssueKey);
                    }
                }
                else
                {
                    _repository.Update(task);
                }
            }
        }

        private EliteTask CreateMapper(EliteTask task, TaskCommand request, EliteTask parentTask = null)
        {
            task.TaskGuid = Guid.NewGuid().ToString();

            if (request.MeetingID != null)
            {
                IsAgendaTopic = true;
            }
            if (request.IsFinalMinutesTasks == true || request.IsReminder == true)
            {

                AddNotification(task, request, parentTask, parentTask is null ? true : false);
            }


            CreateTaskListForTopicHistory(task, request, parentTask is null ? true : false);

            task.Id = request.Id;
            task.Description = request.Description;
            task.ClosureComment = request.ClosureComment;
            task.Title = request.Title;
            task.DueDate = request.DueDate.HasValue ? request.DueDate.Value : (DateTime?)null;
            task.Responsible = JsonConvert.SerializeObject(request.Responsible);
            task.CoResponsibles = request.CoResponsible != null ? JsonConvert.SerializeObject(request.CoResponsible) : null;
            task.CoResponsibleEmailRecipient = request.CoResponsibleEmailRecipient != null ? JsonConvert.SerializeObject(request.CoResponsibleEmailRecipient) : null;
            task.ResponsibleEmailRecipient = JsonConvert.SerializeObject(request.ResponsibleEmailRecipient);
            task.IsCustomEmailRecipient = request.IsCustomEmailRecipient;
            task.ResponsibleDivision = request.ResponsibleDivision;
            if (request.ResponsibleEmailRecipient is null)
            {
                task.IsCustomEmailRecipient = false;
            }
            task.FileLink = request.FileLink;
            task.Status = request.Status;
            task.CommitteeId = request.CommitteeId;
            task.Action = IsUpdate(request.Id) ? (int)DatabaseAction.Update : (int)DatabaseAction.Insert;
            if (request.notifyUsers != null)
                task.IsNotify = request.notifyUsers;

            task.JiraTicketInfo = JsonConvert.SerializeObject(request.JiraTicketInfo);
            task.IsPublishedToJira = isPublishedToJira(request.JiraTicketInfo);
            if (!IsUpdate(request.Id))
            {
                task.MeetingId = request.MeetingID;
                task.MeetingDate = request.MeetingDate;
                task.AgendaId = request.AgendaID;
                task.MeetingStatus = request.MeetingStatus;
            }

            CreateAttachment(task, AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.AddAttachment), _createdBy);
            if (request.Id > 0)
            {
                task.ModifiedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
                task.ModifiedDate = DateTime.Now;
                _modifiedBy = JsonConvert.DeserializeObject<TaskPersonCommand>(task.ModifiedBy);
                _modifiedDate = task.ModifiedDate;
                task.ParentId = parentTask is null ? (long?)null : parentTask.Id;
                DeleteAttachment(AttachmentHelper.GetAttachments(request.Attachments, AttachmnetType.DeleteAttachmentMapping));
            }
            else
            {
                task.CreatedBy = JsonConvert.SerializeObject(_userService.GetUserDetail(securedUID));
                task.CreatedDate = DateTime.Now;
                _createdBy = JsonConvert.DeserializeObject<TaskPersonCommand>(task.CreatedBy);
                _createdDate = task.CreatedDate;
                task.Parent = parentTask is null ? null : parentTask;
            }

            return task;
        }

        private async Task CreateandUpdateSubTaskMapper(IList<TaskCommand> tasks, EliteTask task)
        {
            if (tasks?.Count > 0)
            {
                foreach (var data in tasks)
                {

                    await SaveSubTask(data, task, true);
                }
            }
        }

        private void CreateAttachment(EliteTask eliteTask, IList<TaskAttachmentCommandDto> attachments, TaskPersonCommand user)
        {
            if (attachments?.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    eliteTask.TaskAttachmentMapping.Add(AttachmentMapperAsync(attachment, eliteTask, user));
                }
            }
        }

        private TaskAttachmentMapping AttachmentMapperAsync(TaskAttachmentCommandDto file, EliteTask eliteTask, TaskPersonCommand user)
        {
            TaskAttachmentMapping topicAttachment = new TaskAttachmentMapping();
            topicAttachment.AttachmentName = file.AttachmentDesc;
            topicAttachment.AttachmentGuid = file.AttachmentGuid;
            topicAttachment.AttachmentSize = file.AttachmentSize;
            topicAttachment.CreatedBy = JsonConvert.SerializeObject(user);
            topicAttachment.CreatedDate = DateTime.Now;
            topicAttachment.Task = eliteTask;
            return topicAttachment;
        }

        private void DeleteAttachment(IList<TaskAttachmentCommandDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                foreach (var attachment in attachments)
                {
                    _repository.DeleteAttachment(new TaskAttachmentMapping() { Id = attachment.Id });
                }
            }
        }

        private void PublishThroughEventBusForDeleteAttachments(IList<TaskAttachmentCommandDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForDelete(evt);
            }
        }

        private void PublishThroughEventBusForMappingAttachments(IList<TaskAttachmentCommandDto> attachments)
        {
            if (attachments?.Count > 0)
            {
                var evt = new AttachmentDeleteOrMappingEvent();
                attachments.ToList().ForEach(p => evt.AttachmentGuids.Add(p.AttachmentGuid));
                this._attachmentService.PublishThroughEventBusForMapping(evt);
            }
        }

        private async Task PublishMeetingTask(TaskCommand request, EliteTask task)
        {
            if (IsUpdate(request.Id))
            {
                if (task.MeetingId.HasValue && task.AgendaId.HasValue && task.MeetingId.Value > 0 && task.AgendaId > 0)
                    await this._meetingService.PublishMeetingTaskThroughEventBusAsync(await CreateTaskMapperAsync(request, task.Id));
                //else
                //    throw new EliteException($"{ ((!task.MeetingId.HasValue) ? "Meeting Id was null" : string.Empty)} Or {((!task.AgendaId.HasValue) ? "Agenda Topic Id was null" : string.Empty)}  ");
            }
            else
            {
                if (request.MeetingID.HasValue && request.AgendaID.HasValue && request.MeetingID.Value > 0 && request.AgendaID > 0)
                    await this._meetingService.PublishMeetingTaskThroughEventBusAsync(await CreateTaskMapperAsync(request, task.Id));
                //else
                //    throw new EliteException($"{ ((!request.MeetingID.HasValue) ? "Meeting Id was null" : string.Empty)} Or {((!request.AgendaID.HasValue) ? "Agenda Topic Id was null" : string.Empty)}  ");
            }

        }
        private async Task<MeetingInfo> GetTaskMeetingInfo(long meetingId, long taskid)
        {
            if (meetingId > 0)
                return await _meetingService.GetMeetingInfo((long)meetingId, taskid);
            else
                return null;
        }

        private async Task PublishMeetingAgenda(TaskCommand request, EliteTask task)
        {
            if (IsUpdate(request.Id))
            {
                if (task.MeetingId.HasValue && task.AgendaId.HasValue && task.MeetingId.Value > 0 && task.AgendaId > 0)
                {

                    var topicResponsibles = await _meetingService.GetAgendaById(task.AgendaId.Value);
                    MeetingAgenda existingAgenda = topicResponsibles != null ? JsonConvert.DeserializeObject<MeetingAgenda>(topicResponsibles) : null;

                    MeetingAgendaEvent topicTask = new MeetingAgendaEvent();
                    topicTask.agendaId = request.AgendaID != null ? request.AgendaID.Value : task.AgendaId.Value;
                    topicTask.meetingId = request.MeetingID != null ? request.MeetingID.Value : task.MeetingId.Value;
                    topicTask.responsibleUid = new Responsibles(request.Responsible.Uid.ToUpper());
                    topicTask.coResponsibleUid = new List<Responsibles>();
                    if (request.CoResponsible != null)
                    {
                        foreach (var coresp in request.CoResponsible)
                        {
                            if (coresp.users != null)
                            {
                                foreach (var user in coresp.users)
                                {

                                    topicTask.coResponsibleUid.Add(new Responsibles(user.Uid.ToUpper()));
                                }
                            }
                            else
                            {
                                topicTask.coResponsibleUid.Add(new Responsibles(coresp.Uid.ToUpper()));
                            }
                        }
                    }

                    List<Responsibles> existingAgendaTaskResponsibles = new List<Responsibles>();
                    existingAgendaTaskResponsibles = existingAgenda != null && existingAgenda.TaskResponsibles != null ? JsonConvert.DeserializeObject<List<Responsibles>>(existingAgenda.TaskResponsibles) : null;

                    if (existingAgendaTaskResponsibles != null && existingAgendaTaskResponsibles.Any())
                    {
                        foreach (var coresp in existingAgendaTaskResponsibles)
                        {
                            topicTask.coResponsibleUid.Add(new Responsibles(coresp.Uid.ToUpper()));
                        }
                    }

                    await this._meetingService.PublishMeetingAgendaThroughEventBusAsync(topicTask);
                }

            }
            else
            {
                if (request.MeetingID.HasValue && request.AgendaID.HasValue && request.MeetingID.Value > 0 && request.AgendaID > 0)
                {
                    var topicResponsibles = await _meetingService.GetAgendaById(task.AgendaId.Value);
                    var existingAgenda = topicResponsibles != null ? JsonConvert.DeserializeObject<MeetingAgenda>(topicResponsibles) : null;

                    MeetingAgendaEvent topicTask = new MeetingAgendaEvent();
                    topicTask.agendaId = request.AgendaID != null ? request.AgendaID.Value : task.AgendaId.Value;
                    topicTask.meetingId = request.MeetingID != null ? request.MeetingID.Value : task.MeetingId.Value;
                    topicTask.responsibleUid = new Responsibles(request.Responsible.Uid.ToUpper());
                    topicTask.coResponsibleUid = new List<Responsibles>();
                    if (request.CoResponsible != null)
                    {
                        foreach (var coresp in request.CoResponsible)
                        {
                            if (coresp.users != null)
                            {
                                foreach (var user in coresp.users)
                                {

                                    topicTask.coResponsibleUid.Add(new Responsibles(user.Uid.ToUpper()));
                                }
                            }
                            else
                            {
                                topicTask.coResponsibleUid.Add(new Responsibles(coresp.Uid.ToUpper()));
                            }
                        }
                    }

                    List<Responsibles> existingAgendaTaskResponsibles = new List<Responsibles>();
                    existingAgendaTaskResponsibles = existingAgenda.TaskResponsibles != null ? JsonConvert.DeserializeObject<List<Responsibles>>(existingAgenda.TaskResponsibles) : null;

                    if (existingAgendaTaskResponsibles != null)
                    {
                        foreach (var coresp in existingAgendaTaskResponsibles)
                        {
                            topicTask.coResponsibleUid.Add(new Responsibles(coresp.Uid.ToUpper()));
                        }
                    }
                    await this._meetingService.PublishMeetingAgendaThroughEventBusAsync(topicTask);
                }

            }

        }

        private async Task<MeetingTaskEvent> CreateTaskMapperAsync(TaskCommand command, long taskId)
        {
            List<MeetingMinuteTaskEvent> subtask = new List<MeetingMinuteTaskEvent>();
            if (delSubTaskIds.Count > 0)
            {
                if (command != null)
                {
                    var newtaskEntity = await _repository.GetSubTaskListByIdAsync(taskId);
                    command.SubTask.Where(x => delSubTaskIds.Contains(x.Id)).ToList().ForEach(x => subtask.Add(new MeetingMinuteTaskEvent()
                    {
                        ActionType = (int)(EnumMinuteActionStatus.Task),
                        AgendaTopicId = x.AgendaID != null ? x.AgendaID : null,
                        Description = x.Description != null ? x.Description : null,
                        CommitteeId = x.CommitteeId != null ? x.CommitteeId : null,
                        CreatedBy = _createdBy,
                        CreatedDate = _createdDate,
                        DueDate = x.DueDate,
                        MeetingId = x.MeetingID != null ? x.MeetingID : null,
                        ModifiedBy = _modifiedBy,
                        ModifiedDate = _modifiedDate,
                        Responsible = x.Responsible,
                        SubTaskCount = 0,
                        TaskId = x.Id,
                        Title = x.Title,
                        Status = x.Status,
                        ParentId = x.ParentId,
                        IsDeleted = x.IsDeleted,
                        Action = (int)DatabaseAction.Delete
                    }));
                }
            }
            var subtaskEntity = await _repository.GetSubTaskListByIdAsync(taskId);
            if (subtaskEntity != null)
            {
                subtaskEntity.ForEach(x => subtask.Add(new MeetingMinuteTaskEvent()
                {
                    ActionType = (int)(EnumMinuteActionStatus.Task),
                    AgendaTopicId = x.AgendaId != null ? x.AgendaId : null,
                    Description = x.Description != null ? x.Description : null,
                    CommitteeId = x.CommitteeId != null ? x.CommitteeId : null,
                    CreatedBy = JsonConvert.DeserializeObject<TaskPersonCommand>(x.CreatedBy),
                    CreatedDate = x.CreatedDate,
                    DueDate = x.DueDate,
                    MeetingId = x.MeetingId != null ? x.MeetingId : null,
                    ModifiedBy = x.ModifiedBy != null ? (JsonConvert.DeserializeObject<TaskPersonCommand>(x.ModifiedBy)) : null,
                    ModifiedDate = x.ModifiedDate.HasValue ? x.ModifiedDate.Value : (DateTime?)null,
                    Responsible = JsonConvert.DeserializeObject<TaskPersonCommand>(x.Responsible),
                    SubTaskCount = 0,
                    TaskId = x.Id,
                    Title = x.Title,
                    Status = x.Status,
                    ParentId = x.ParentId,
                    Action = updateSubTaskIds.Contains(x.Id) ? (int)DatabaseAction.Update : (int)DatabaseAction.Insert
                }));
            }

            //List<MeetingMinuteTaskEvent> subtask
            return new MeetingTaskEvent()
            {

                RequestId = command.Id,
                // subtask = 
                Task = new MeetingMinuteTaskEvent()
                {
                    ActionType = (int)(EnumMinuteActionStatus.Task),
                    AgendaTopicId = command.AgendaID,
                    Description = command.Description,
                    CommitteeId = command.CommitteeId,
                    CreatedBy = _createdBy,
                    CreatedDate = _createdDate,
                    DueDate = command.DueDate,
                    MeetingId = command.MeetingID,
                    ModifiedBy = _modifiedBy,
                    ModifiedDate = _modifiedDate,
                    Responsible = command.Responsible,
                    CoResponsible = command.CoResponsible,
                    SubTaskCount = command.SubTask?.Count > 0 ? command.SubTask.Where(p => p.IsDeleted == false).Count() : 0,
                    TaskId = taskId,
                    Title = command.Title,
                    Status = command.Status,
                    Subtask = subtask,
                    ClosureComment = command.ClosureComment,
                    Attachments = command.Attachments?.Count > 0 ?
                command.Attachments.ToList().Select(a =>
                       new MeetingTaskAttachmentEvent()
                       {
                           attachmentDesc = a.AttachmentDesc,
                           attachmentGuid = a.AttachmentGuid,
                           attachmentSize = a.AttachmentSize,
                           createdBy = _createdBy,
                           createdDate = System.DateTime.Now,
                           Isdeleted = a.IsDeleted
                       }).ToList<MeetingTaskAttachmentEvent>()
                : null
                }
            };
        }

        private void InitializeUsers()
        {
            Task.Run(async () =>
            {
                this.committees = await this._userService.GetCommitees();

            }).Wait();
        }
        private string GetCommitteeName(long? committeeId)
        {
            try
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
            catch (Exception e)
            {
                return null;
            }

        }

        private string GetCommitteePoolIdEmail(long? committeeId)
        {
            try
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
            catch (Exception e)
            {
                return null;
            }
        }

        private string GetCommitteePoolIdName(long? committeeId)
        {
            try
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
            catch (Exception e)
            {
                return null;
            }
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

        private void AddNotification(EliteTask task, TaskCommand request, EliteTask parentTask = null, bool isMainTask = false)
        {
            List<TaskPersonCommand> removedCoResponsible = new List<TaskPersonCommand>();
            List<TaskPersonCommand> addedCoResponsible = new List<TaskPersonCommand>();

            if (parentTask != null)
            {
                if (IsUpdate(task.Id))
                {
                    if (!(request.IsDeleted.HasValue && request.IsDeleted.Value))
                    {
                        postForUpdateSubTaskResponsibleNotification(task, request);
                    }
                }
                else
                    postForCreateSubTaskResponsibleNotification(task, request);
            }
            else
            {
                if (IsUpdate(task.Id))
                {
                    bool isStatusUpdate = statusUpdated((long)task.Status, (long)request.Status);
                    if (isStatusUpdate && (request.Status.Equals((int)CommonLib.TaskStatus.Completed) || request.Status.Equals((int)CommonLib.TaskStatus.InProgress)) && request.CommitteeId != null)
                    {

                        if (!(task.Status.Equals((int)CommonLib.TaskStatus.Assigned) && request.Status.Equals((int)CommonLib.TaskStatus.InProgress)))
                        {
                            // if the task status is changed from assigned to inprogress then no need to send the notification
                            if (!isEliteClassic)
                            {
                                postForCompleteCommitteManagersNotification(task, request);
                            }
                        }
                    }
                    switch (request.notifyUsers)
                    {
                        case (long)Notify.DO_NOT_NOTIFY:
                            break;
                        case (long)Notify.ONLY_CO_RESPONSIBLE:
                            postForUpdateCoResponsibleNotification(task, request);
                            break;
                        case (long)Notify.ONLY_RESPONSIBLE:
                            postForUpdateResponsibleNotification(task, request);
                            break;
                        case (long)Notify.NOTIFY_ALL:
                            postForUpdateCoResponsibleNotification(task, request);
                            postForUpdateResponsibleNotification(task, request);

                            break;
                        default:
                            break;
                    }

                }
                else
                {

                    switch (request.notifyUsers)
                    {
                        case (short)Notify.DO_NOT_NOTIFY:
                            break;
                        case (short)Notify.ONLY_CO_RESPONSIBLE:
                            postForCreateCoResponsibleNotification(task, request);
                            break;
                        case (short)Notify.ONLY_RESPONSIBLE:
                            postForCreateResponsibleNotification(task, request);
                            break;
                        case (short)Notify.NOTIFY_ALL:
                            postForCreateResponsibleNotification(task, request);
                            postForCreateCoResponsibleNotification(task, request);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void postForCompleteCommitteManagersNotification(EliteTask task, TaskCommand request)
        {
            string _description = GetDescription(task.Description, request.Description);
            //CommitteeManagersMailIdsDto committeeManagersMailIds = _userService.GetCommitteeManagersMailIds(request.CommitteeId.Value, _modifiedBy.Uid, _createdBy.Uid).Result;
            //var status = 0;
            InitializeUsers();
            //if (committeeManagersMailIds.CommitteeMailIds != null)
            //{
            this._taskNotifications.Add(new EmailNotificationEvent
            {
                NotificationID = new Guid(task.TaskGuid),
                ActionType = task.Status.Equals((int)CommonLib.TaskStatus.Completed) && request.Status.Equals((int)CommonLib.TaskStatus.InProgress) ?
                NotificationActionType.REJECT_COMPLETE_INPROGRESS :
                _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE,
                Message = new TaskEvent
                {
                    CreatedBy = _createdBy,
                    Responsible = _createdBy,
                    TaskTitle = request.Title,
                    TaskId = task.Id,
                    Description = _description,
                    DueDate = request.DueDate,
                    CommitteeName = GetCommitteeName(task.CommitteeId),
                    ClosureComment = request.ClosureComment,
                    RejectionComment = request.RejectionComment,
                    Role = GetUserRole(_createdBy.Uid, task.CommitteeId),
                    ModifiedBy = _modifiedBy != null ? _modifiedBy : null,
                    status = 15,
                    IsDemoUser = request.IsDemoUser,
                    MeetingId = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingId)) ? Convert.ToInt64(meetingInfo.MeetingId) : 0,
                    MeetingName = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingName)) ? meetingInfo.MeetingName : string.Empty,
                    MeetingDatetime = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingDate)) ? meetingInfo.MeetingDate : string.Empty
                },
                GroupID = GroupType.TASK,
            });
            //}
            return;
        }

        private void postForCreateCoResponsibleNotification(EliteTask task, TaskCommand request)
        {
            string description = request.Description != null ? request.Description : string.Empty;
            string userRole = GetUserRole(_createdBy.Uid, request.CommitteeId);

            var currentList = request.CoResponsible != null ? request.CoResponsible.ToList<TaskGroupCommand>() : null;
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            if (request.IsCustomEmailRecipient != null)
            {
                if (request.IsCustomEmailRecipient == true && request.CoResponsibleEmailRecipient != null)
                {
                    currentList = request.CoResponsibleEmailRecipient != null ? request.CoResponsibleEmailRecipient.ToList<TaskGroupCommand>() : null;
                }
            }

            if (currentList != null)
                foreach (var item in currentList)
                {
                    if (item.users != null)
                    {
                        foreach (var user in item.users)
                        {
                            var coresponsible = new TaskPersonCommand(user.Uid, user.DisplayName);
                            if (!(request.notifyUsers == (long)Notify.NOTIFY_ALL && (coresponsible.Uid.ToUpper() == request.Responsible.Uid.ToUpper())))
                                AddTaskEmailNotification(task, NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE, _createdBy,
                                    coresponsible, request.Title, task.Id, description, request.DueDate, request.CommitteeId, committeeName,
                                    userRole, null, request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                        }
                    }
                    else
                    {
                        var coresponsible = new TaskPersonCommand(item.Uid, item.DisplayName);
                        if (!(request.notifyUsers == (long)Notify.NOTIFY_ALL && (coresponsible.Uid.ToUpper() == request.Responsible.Uid.ToUpper())))
                            AddTaskEmailNotification(task, NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE, _createdBy, coresponsible,
                                request.Title, task.Id, description, request.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                                null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                    }
                }
        }

        private void postForCreateResponsibleNotification(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            string description = request.Description != null ? request.Description : string.Empty;
            string userRole = GetUserRole(_createdBy.Uid, request.CommitteeId);

            var responsible = new TaskPersonCommand(request.Responsible.Uid, request.Responsible.DisplayName);
            if (request.IsCustomEmailRecipient != null)
            {
                if (request.IsCustomEmailRecipient == true && request.ResponsibleEmailRecipient != null)
                {
                    responsible = new TaskPersonCommand(request.ResponsibleEmailRecipient.Uid, request.ResponsibleEmailRecipient.DisplayName);
                }
            }

            AddTaskEmailNotification(task, NotificationActionType.ASSIGN_TO_RESPONSIBLE, _createdBy, responsible,
                request.Title, task.Id, description, request.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
        }

        private void postForUpdateResponsibleNotification(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList, false);
            var existingResponsible = JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible);
            var responsibleEmailRecipient = task.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<TaskPersonCommand>(task.ResponsibleEmailRecipient) : null;
            if (task.IsCustomEmailRecipient != null)
            {
                if (task.IsCustomEmailRecipient == true && responsibleEmailRecipient != null)
                {
                    existingResponsible = responsibleEmailRecipient;
                }
            }
            var responsible = new TaskPersonCommand(request.Responsible.Uid, request.Responsible.DisplayName);
            if (request.IsCustomEmailRecipient != null)
            {
                if (request.IsCustomEmailRecipient == true && request.ResponsibleEmailRecipient != null)
                {
                    responsible = new TaskPersonCommand(request.ResponsibleEmailRecipient.Uid, request.ResponsibleEmailRecipient.DisplayName);
                }
            }
            string description = GetDescription(task.Description, request.Description);
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);

            bool flagUpdated = flagsUpdated((long)task.IsNotify, (long)request.notifyUsers);
            bool statusUpdate = statusUpdated((long)task.Status, (long)request.Status);

            // Determine whether to set isUpdate true based on condition
            bool setIsUpdateTrue = false;
            if (flagUpdated == false && statusUpdate == false &&
                existingResponsible.Uid.Trim().ToUpper() == responsible.Uid.Trim().ToUpper()
                && request.IsReminder != true)
            {
                setIsUpdateTrue = true;
            }

            //if notify flags have not updated deletion mails will trigger as we take care of delta
            if ((flagUpdated == false && statusUpdate == false) || (flagUpdated == false && statusUpdate == true && (existingResponsible.Uid.ToUpper() != responsible.Uid.ToUpper())))
            {
                var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_RESPONSIBLE;
                var existingTaskParticipants = new TaskPeople(respList, coRespList, setIsUpdateTrue);
                //send notification to the existing responsible
                AddTaskEmailNotification(task, actionType, _createdBy, responsible, request.Title, task.Id,
                    description, request.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                    committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, existingTaskParticipants);

                if (existingResponsible.Uid.Trim().ToUpper() != responsible.Uid.Trim().ToUpper())
                {
                    userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, task.CommitteeId);
                    var taskExisitingPeople = new TaskPeople(existingResponsible, coRespList, false);
                    // if delta add notification for old responsible
                    AddTaskEmailNotification(task, NotificationActionType.NOTIFY_OLD_RESPONSIBLE, _createdBy, existingResponsible,
                        request.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, responsible.DisplayName,
                        request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskExisitingPeople);
                }
            }
            if (statusUpdate == true)
            {
                statusMailResponsible(task, request);
            }
            if (flagUpdated == true && statusUpdate == false)
            {
                var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_RESPONSIBLE;
                userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);
                //send notification to the existing responsible if
                //flags changed and no need to take care of delta
                AddTaskEmailNotification(task, actionType, _createdBy, responsible, request.Title, task.Id,
                    description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                    committeePoolIdEmail, null, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
            }
        }

        private void postForUpdateCoResponsibleNotification(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList, false);
            string description = GetDescription(task.Description, request.Description);
            var createdBy = JsonConvert.DeserializeObject<TaskPersonCommand>(task.CreatedBy);
            var existingCoResponsiblelist = task.CoResponsibles != null ? JsonConvert.DeserializeObject<List<TaskGroupCommand>>(task.CoResponsibles) : null;
            var coResponsibleEmailRecipient = task.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<TaskGroupCommand>>(task.CoResponsibleEmailRecipient) : null;
            if (task.IsCustomEmailRecipient != null)
            {
                if (task.IsCustomEmailRecipient == true && coResponsibleEmailRecipient != null && coResponsibleEmailRecipient.Count > 0)
                {
                    existingCoResponsiblelist = coResponsibleEmailRecipient;
                }
            }
            var existingCoResponsibleUsersList = new List<TaskPersonCommand>();
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);

            if (existingCoResponsiblelist != null && existingCoResponsiblelist.Count > 0)
                foreach (var coresponsible in existingCoResponsiblelist)
                {

                    if (coresponsible.users != null)
                    {
                        foreach (var user in coresponsible.users)
                        {
                            existingCoResponsibleUsersList.Add(user);
                        }
                    }
                    else
                        existingCoResponsibleUsersList.Add(new TaskPersonCommand(coresponsible.Uid, coresponsible.DisplayName));
                }

            var currentList = request.CoResponsible != null ? request.CoResponsible.ToList<TaskGroupCommand>() : null;
            if (request.IsCustomEmailRecipient != null)
            {
                if (request.IsCustomEmailRecipient == true && request.CoResponsibleEmailRecipient != null && request.CoResponsibleEmailRecipient.Count > 0)
                {
                    currentList = request.CoResponsibleEmailRecipient != null ? request.CoResponsibleEmailRecipient.ToList<TaskGroupCommand>() : null;
                }
            }
            var currentListUsers = new List<TaskPersonCommand>();

            if (currentList != null && currentList.Count > 0)
                foreach (var coresponsible in currentList)
                {
                    if (coresponsible.users != null)
                    {
                        foreach (var user in coresponsible.users)
                        {
                            currentListUsers.Add(user);
                        }
                    }
                    else
                        currentListUsers.Add(new TaskPersonCommand(coresponsible.Uid, coresponsible.DisplayName));
                }

            List<TaskPersonCommand> responsibletoRemove = null;
            List<TaskPersonCommand> responsibletoAdd = null;

            // Determine users to remove
            if (existingCoResponsibleUsersList != null && existingCoResponsibleUsersList.Count > 0)
            {
                responsibletoRemove = existingCoResponsibleUsersList
                    .Where(x => !currentListUsers.Any(y => y.Uid == x.Uid))
                    .ToList();
            }

            // Determine users to add
            if (currentListUsers != null && currentListUsers.Count > 0)
            {
                responsibletoAdd = currentListUsers
                    .Where(x => !existingCoResponsibleUsersList.Any(y => y.Uid == x.Uid))
                    .ToList();
            }

            if (request.MeetingStatus.HasValue && request.MeetingStatus.Value == 1 && request.Status == 1)
            {
                // For meeting tasks, ignore deltas and always send to current list of co-responsibles
                responsibletoAdd = currentListUsers != null ? currentListUsers.ToList() : new List<TaskPersonCommand>();

                // Ensure removed list is still captured (those not in the new list)
                responsibletoRemove = existingCoResponsibleUsersList != null && currentListUsers != null
                    ? existingCoResponsibleUsersList
                        .Where(x => !currentListUsers.Any(y => y.Uid == x.Uid))
                        .ToList()
                    : new List<TaskPersonCommand>();
            }

            bool flagUpdated = flagsUpdated((long)task.IsNotify, (long)request.notifyUsers);
            bool statusUpdate = statusUpdated((long)task.Status, (long)request.Status);

            // Check if there is any common co-responsible UID
            bool hasCommonCoResponsible = existingCoResponsibleUsersList.Any(existingUser =>
                currentListUsers.Any(currentUser => string.Equals(existingUser.Uid, currentUser.Uid, StringComparison.OrdinalIgnoreCase))
            );

            // Set isUpdate to true only if flags and status are unchanged AND there is a common co-responsible UID

            // Set isUpdate to true if normal conditions are met OR if meeting scenario applies
            var existingTaskParticipants = new TaskPeople(respList, coRespList,
                ((flagUpdated == false && statusUpdate == false && hasCommonCoResponsible && request.IsReminder != true)));

            if ((flagUpdated == false && statusUpdate == false) || (flagUpdated == false && statusUpdate == true && ((responsibletoAdd != null && responsibletoAdd.Count > 0) || (responsibletoRemove != null && responsibletoRemove.Count > 0))))
            {
                // 1. Notify removed co-responsibles
                if (responsibletoRemove != null && responsibletoRemove.Count > 0)
                {
                    foreach (var item in responsibletoRemove)
                    {
                        var coresponsible = new TaskPersonCommand(item.Uid, item.DisplayName);
                        // For removed co-responsibles, the isUpdate flag in TaskPeople should be false.
                        // A new TaskPeople object is created for each removed user to ensure isUpdate is false for them.
                        var removedCoResponsibleParticipants = new TaskPeople(respList, new List<TaskPersonCommand> { coresponsible }, false);
                        AddTaskEmailNotification(task, NotificationActionType.NOTIFY_OLD_CO_RESPONSIBLE, _createdBy,
                            coresponsible, request.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null,
                            request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, removedCoResponsibleParticipants);
                    }
                }

                // 2. Notify newly added co-responsibles
                if (responsibletoAdd != null && responsibletoAdd.Count > 0)
                {
                    foreach (var item in responsibletoAdd)
                    {
                        var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE;
                        var coresponsible = new TaskPersonCommand(item.Uid, item.DisplayName);
                        userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, task.CommitteeId);

                        if (!(request.notifyUsers == (long)Notify.NOTIFY_ALL && (coresponsible.Uid.ToUpper() == request.Responsible.Uid.ToUpper())))
                        {
                            // For newly added co-responsibles, isUpdate should be false as it's an assignment, not an update to an existing assignment.
                            var newCoResponsibleParticipants = new TaskPeople(respList, new List<TaskPersonCommand> { coresponsible }, false);
                            AddTaskEmailNotification(task, actionType, _createdBy, coresponsible, request.Title, task.Id,
                                description, task.DueDate, request.CommitteeId, committeeName, userRole, request.Responsible.DisplayName,
                                request.IsDemoUser, meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, newCoResponsibleParticipants);
                        }
                    }
                }
            }

            // 3. Notify EXISTING co-responsibles if existingTaskParticipants.IsUpdate is true

            if ((request.MeetingStatus ?? 0) == 1 && request.Status == 1)
            {
                if (existingTaskParticipants.IsUpdateOperation)
                {
                    if (currentListUsers != null && currentListUsers.Count > 0)
                    {
                        foreach (var item in currentListUsers)
                        {
                            // Ensure this user was an existing co-responsible (i.e., not a newly added one)
                            if (!responsibletoAdd.Any(u => u.Uid == item.Uid))
                            {
                                var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE;
                                userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, task.CommitteeId);

                                if (!(request.notifyUsers == (long)Notify.NOTIFY_ALL && (item.Uid.ToUpper() == request.Responsible.Uid.ToUpper())))
                                    AddTaskEmailNotification(task, actionType, _createdBy, item, request.Title, task.Id, description,
                                        request.DueDate, request.CommitteeId, committeeName, userRole, request.Responsible.DisplayName, request.IsDemoUser,
                                        meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, existingTaskParticipants);
                            }
                        }
                    }
                }
            }

            if (statusUpdate == true)
            {
                statusMailCoResponsible(task, request);
            }
            if (flagUpdated == true && statusUpdate == false)
            {
                //When flags changes send only to current list of co responsibles
                if (currentList != null)
                    foreach (var item in currentList)
                    {
                        var coresponsible = new TaskPersonCommand(item.Uid, item.DisplayName);
                        if (item.users != null)
                        {
                            foreach (var user in item.users)
                            {
                                var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE;
                                userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, task.CommitteeId);
                                //add notification for the new co-responsible
                                if (!(request.notifyUsers == (long)Notify.NOTIFY_ALL && (coresponsible.Uid.ToUpper() == request.Responsible.Uid.ToUpper())))
                                    AddTaskEmailNotification(task, actionType, _createdBy, user, request.Title, task.Id, description,
                                        task.DueDate, request.CommitteeId, committeeName, userRole, request.Responsible.DisplayName, request.IsDemoUser,
                                        meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                            }
                        }
                        else
                        {
                            userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, task.CommitteeId);
                            var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE;
                            //add notification for the new co-responsible
                            AddTaskEmailNotification(task, actionType, _createdBy, coresponsible, request.Title, task.Id,
                                description, task.DueDate, request.CommitteeId, committeeName, userRole, request.Responsible.DisplayName,
                                request.IsDemoUser, meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                        }
                    }
            }
        }

        private void statusMailCoResponsible(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            string description = GetDescription(task.Description, request.Description);
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);
            List<TaskGroupCommand> coResponsibleList = new List<TaskGroupCommand>();

            if (request.IsCustomEmailRecipient != null && request.IsCustomEmailRecipient == true && request.CoResponsibleEmailRecipient.Count > 0)
            {
                coResponsibleList = request.CoResponsibleEmailRecipient;
            }
            else
            {
                request.CoResponsible.ForEach(x =>
                {
                    coResponsibleList.Add(x);
                });
            }

            //current responsible editing
            if (task.Status.Equals((int)CommonLib.TaskStatus.Assigned) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
            {
                if (request.CoResponsible != null)
                {
                    foreach (var responsible in coResponsibleList)
                    {
                        var coresponsible = new TaskPersonCommand(responsible.Uid, responsible.DisplayName);

                        if (responsible.users != null)
                        {
                            foreach (var user in responsible.users)
                            {
                                var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                                //if (request.Responsible.Uid.ToUpper() != user.Uid.ToUpper())
                                AddTaskEmailNotification(task, actionType, _createdBy, user, request.Title, task.Id,
                                        description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                        committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                            }
                        }
                        else
                        {
                            var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                            //if (request.Responsible.Uid.ToUpper() != coresponsible.Uid.ToUpper())
                            AddTaskEmailNotification(task, actionType, _createdBy, coresponsible, request.Title, task.Id,
                                   description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                   committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                        }
                    }
                }
                return;
            }
            else if (task.Status.Equals((int)CommonLib.TaskStatus.InProgress) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
            {
                if (coResponsibleList != null)
                {
                    foreach (var responsible in coResponsibleList)
                    {
                        var coresponsible = new TaskPersonCommand(responsible.Uid, responsible.DisplayName);

                        if (responsible.users != null)
                        {
                            foreach (var user in responsible.users)
                            {
                                var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                                AddTaskEmailNotification(task, actionType, _createdBy, user, request.Title, task.Id,
                                    description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                    committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                            }
                        }
                        else
                        {
                            var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                            AddTaskEmailNotification(task, actionType, _createdBy, coresponsible, request.Title, task.Id,
                                description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                        }
                    }
                }
                return;
            }
            else if (task.Status.Equals((int)CommonLib.TaskStatus.Completed) && request.Status.Equals((int)CommonLib.TaskStatus.InProgress))
            {
                if (coResponsibleList != null)
                {
                    foreach (var responsible in coResponsibleList)
                    {
                        var coresponsible = new TaskPersonCommand(responsible.Uid, responsible.DisplayName);

                        if (responsible.users != null)
                        {
                            foreach (var user in responsible.users)
                            {
                                var actionType = NotificationActionType.REJECT_COMPLETE_INPROGRESS;
                                AddTaskEmailNotification(task, actionType, _createdBy, user, request.Title, task.Id,
                                    description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                    committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, rejectionComment: request.RejectionComment, isCoResponsible: false, taskPeople);
                            }
                        }
                        else
                        {
                            var actionType = NotificationActionType.REJECT_COMPLETE_INPROGRESS;
                            AddTaskEmailNotification(task, actionType, _createdBy, coresponsible, request.Title, task.Id,
                                description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser, meetingInfo,
                                committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, rejectionComment: request.RejectionComment, isCoResponsible: false, taskPeople);
                        }
                    }
                }
                return;
            }
        }

        private void statusMailResponsible(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            string description = GetDescription(task.Description, request.Description);
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);
            var responsible = new TaskPersonCommand(request.Responsible.Uid, request.Responsible.DisplayName);
            if (request.IsCustomEmailRecipient != null)
            {
                if (request.IsCustomEmailRecipient == true && request.ResponsibleEmailRecipient != null)
                {
                    responsible = new TaskPersonCommand(request.ResponsibleEmailRecipient.Uid, request.ResponsibleEmailRecipient.DisplayName);
                }
            }

            //current responsible editing
            if (task.Status.Equals((int)CommonLib.TaskStatus.Assigned) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
            {
                var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                AddTaskEmailNotification(task, actionType, _createdBy, responsible, request.Title,
                    task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                    meetingInfo, committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                return;
            }
            else if (task.Status.Equals((int)CommonLib.TaskStatus.InProgress) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
            {
                var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_INPROGRESS_TO_COMPLETE : NotificationActionType.INPROGRESS_TO_COMPLETE;
                AddTaskEmailNotification(task, actionType, _createdBy, responsible, request.Title,
                    task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                    meetingInfo, committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                return;
            }
            else if (task.Status.Equals((int)CommonLib.TaskStatus.Completed) && request.Status.Equals((int)CommonLib.TaskStatus.InProgress))
            {
                var actionType = NotificationActionType.REJECT_COMPLETE_INPROGRESS;
                AddTaskEmailNotification(task, actionType, _createdBy, responsible, request.Title,
                    task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                    meetingInfo, committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, rejectionComment: request.RejectionComment, isCoResponsible: false, taskPeople);
                return;
            }
        }

        private void postForCreateSubTaskResponsibleNotification(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            string description = GetDescription(task.Description, request.Description);
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);

            AddTaskEmailNotification(task, NotificationActionType.ASSIGN_TO_RESPONSIBLE, _createdBy,
                request.Responsible, request.Title, task.Id, description, request.DueDate, request.CommitteeId, committeeName,
                userRole, null, request.IsDemoUser, meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, taskPeople);
        }

        private void postForUpdateSubTaskResponsibleNotification(EliteTask task, TaskCommand request)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            var existingResponsible = JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible);
            var responsible = new TaskPersonCommand(request.Responsible.Uid, request.Responsible.DisplayName);
            string description = GetDescription(task.Description, request.Description);
            string userRole = GetUserRole(_createdBy.Uid, request.CommitteeId);
            bool statusUpdate = statusUpdated((long)task.Status, (long)request.Status);
            // Determine whether to set isUpdate true based on condition
            bool setIsUpdateTrue = false;
            if (statusUpdate == false && existingResponsible.Uid.Trim().ToUpper() == responsible.Uid.Trim().ToUpper()
                && request.IsReminder != true)
            {
                setIsUpdateTrue = true;
            }

            if ((statusUpdate == false || (statusUpdate == true && (existingResponsible.Uid.ToUpper() != responsible.Uid.ToUpper()))) && task.Status != 3)
            {
                var actionType = meetingInfo != null ? request.IsCorporateEmail ? NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE_CORPORATE : NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE : NotificationActionType.ASSIGN_TO_RESPONSIBLE;
                var existingTaskParticipants = new TaskPeople(respList, coRespList, setIsUpdateTrue);
                AddTaskEmailNotification(task, actionType, _createdBy, request.Responsible, request.Title,
                    task.Id, description, request.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                    meetingInfo, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, existingTaskParticipants);

                if (existingResponsible.Uid.ToUpper() != responsible.Uid.ToUpper())
                {
                    userRole = GetUserRole(_modifiedBy != null ? _modifiedBy.Uid : _createdBy.Uid, request.CommitteeId);
                    // if delta add notification for old responsible
                    AddTaskEmailNotification(task, NotificationActionType.NOTIFY_OLD_RESPONSIBLE, _createdBy,
                        existingResponsible, request.Title, task.Id, description, request.DueDate, request.CommitteeId, committeeName,
                        userRole, responsible.DisplayName, request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, existingTaskParticipants);
                }
            }

            if (statusUpdate == true)
            {
                //current responsible editing
                if (task.Status.Equals((int)CommonLib.TaskStatus.Assigned) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
                {
                    userRole = GetUserRole(_createdBy.Uid, request.CommitteeId);
                    var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_OPEN_TO_COMPLETE : NotificationActionType.OPEN_TO_COMPLETE;
                    AddTaskEmailNotification(task, actionType, _createdBy, request.Responsible, request.Title,
                        task.Id, description, request.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                        meetingInfo, committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, taskPeople);
                    return;
                }
                else if (task.Status.Equals((int)CommonLib.TaskStatus.InProgress) && request.Status.Equals((int)CommonLib.TaskStatus.Completed))
                {
                    userRole = GetUserRole(_createdBy.Uid, request.CommitteeId);
                    var actionType = _createdBy.Uid.ToUpper().Equals(this.UID) ? NotificationActionType.CREATOR_INPROGRESS_TO_COMPLETE : NotificationActionType.INPROGRESS_TO_COMPLETE;
                    AddTaskEmailNotification(task, actionType, _createdBy, request.Responsible, request.Title,
                        task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null, request.IsDemoUser,
                        meetingInfo, committeePoolIdEmail, _modifiedBy, 15, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, taskPeople);
                    return;
                }
            }
        }

        private bool flagsUpdated(long oldFlag, long newFlag)
        {
            if (oldFlag == newFlag)
                return false;
            if ((oldFlag == (long)Notify.ONLY_RESPONSIBLE || oldFlag == (long)Notify.ONLY_CO_RESPONSIBLE) && newFlag == (long)Notify.NOTIFY_ALL)
                return false;

            return true;
        }

        private bool statusUpdated(long oldStatus, long newStatus)
        {
            if (oldStatus == newStatus)
                return false;
            return true;
        }

        private async Task AddDeleteNotification(EliteTask task, TaskCommand request, bool isSubTask = false)
        {
            var respList = request.Responsible ?? new TaskPersonCommand("", "");
            var coRespList = request.CoResponsible != null
                ? request.CoResponsible.Select(r => new TaskPersonCommand(r.Uid, r.DisplayName)).ToList()
                : new List<TaskPersonCommand>();
            var taskPeople = new TaskPeople(respList, coRespList);
            string description = GetDescription(task.Description, request.Description);
            var user = await _userService.GetUser(this.UID.ToUpper());
            string userRole = GetUserRole(_createdBy.Uid, task.CommitteeId);

            if (isSubTask)
                if (request.IsDeleted.HasValue && request.IsDeleted.Value)
                {
                    var taskResponsible = JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible);
                    var subTaskPeople = new TaskPeople(taskResponsible, new List<TaskPersonCommand>());
                    AddTaskEmailNotification(task, NotificationActionType.SUBTASK_TO_DELETE, user, taskResponsible,
                        task.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null,
                        request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, subTaskPeople);

                    if (request.CoResponsible != null)
                        foreach (var responsible in request.CoResponsible)
                        {
                            var coresponsible = new TaskPersonCommand(responsible.Uid, responsible.DisplayName);
                            if (responsible.users != null)
                                foreach (var userResp in responsible.users)
                                {
                                    AddTaskEmailNotification(task, NotificationActionType.TASK_TO_DELETE, userResp,
                                        userResp, task.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName,
                                        userRole, null, request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: false, taskPeople);
                                }
                            else
                                AddTaskEmailNotification(task, NotificationActionType.TASK_TO_DELETE, user, coresponsible,
                                    task.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null,
                                    request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, taskPeople);
                        }
                }

            if (task.InverseParent?.Count > 0 || (user != null && task != null))
            {
                foreach (var inverseParent in task.InverseParent)
                {
                    var taskResponsible = JsonConvert.DeserializeObject<TaskPersonCommand>(inverseParent.Responsible);
                    var subTaskPeople = new TaskPeople(taskResponsible, new List<TaskPersonCommand>());
                    AddTaskEmailNotification(task, NotificationActionType.SUBTASK_TO_DELETE, user, taskResponsible,
                        inverseParent.Title, task.Id, description, task.DueDate, request.CommitteeId, committeeName, userRole, null,
                        request.IsDemoUser, null, committeePoolIdEmail, _modifiedBy, null, committeePoolIdName, closureComment: request.ClosureComment, null, isCoResponsible: true, subTaskPeople);
                }
            }
        }

        private void CreateTaskListForTopicHistory(EliteTask task, TaskCommand request, bool isMainTask = false)
        {
            if (IsAgendaTopic)
            {
                if (isMainTask)
                {
                    _protocolTaskList.Add(new ProtocolTask
                    {
                        IsDeleted = false,
                        TaskCategory = ProtocolTask.TaskType.MainTask,
                        TaskGuid = new Guid(task.TaskGuid),
                        TopicId = request.AgendaID,
                        Comments = request.Description,
                        CreatedBy = _createdBy,

                    });
                }
            }
        }

        private async Task ProcessEventStore(EliteTask task)
        {
            // ProcessMotification();
            var eventStores = new List<Elite.EventBus.EventStore.EliteEventStoreDto>();
            this._taskNotifications.ToList().ForEach(p =>
            {
                var _event = p as EmailNotificationEvent;
                _event.Description = task.Description;
                _event.TaskId = task.Id;
                _event.DueDate = task.DueDate;

                long? id = 0;
                if (_event.NotificationID.ToString().Equals(task.TaskGuid.ToString()))
                {
                    id = task.Id;
                }
                else if (task.InverseParent?.Count > 0)
                {
                    var data = task.InverseParent.FirstOrDefault(x => x.TaskGuid.ToString().Equals(_event.NotificationID.ToString()));

                    if (data != null)
                    {
                        id = data.ParentId;
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
                    Sourcetypeid = id.HasValue ? id.Value : 0
                });
            });

            if (eventStores?.Count > 0)
            {

                await this._integrationEventStoreService.SaveEventAsync(eventStores
               , ((EliteTaskContext)_repository.UnitOfWork).Database.CurrentTransaction.GetDbTransaction());
            }
        }

        private async Task ProcessProtocolTask(EliteTask task)
        {
            await Task.Run(() =>
            {
                this._protocolTaskList.ToList().ForEach(p =>
                {
                    long id = 0;
                    bool maintask = false, updateflag = false;

                    if (p.TaskGuid.ToString().Equals(task.TaskGuid.ToString()))
                    {
                        id = task.Id;
                        p.TaskId = id;
                    }
                    else if (task.InverseParent?.Count > 0)
                    {
                        var data = task.InverseParent.FirstOrDefault(x => x.TaskGuid.ToString().Equals(p.TaskGuid.ToString()));

                        if (data != null)
                        {
                            id = data.Id;
                            p.TaskId = id;

                        }
                    }

                    updateflag = IsUpdate(requestId);
                    maintask = p.TaskCategory == ProtocolTask.TaskType.MainTask ? true : false;
                    //for main task

                    StringBuilder addinfo = new StringBuilder();
                    addinfo.Append("<p>");
                    if (!string.IsNullOrEmpty(task.Title))
                        addinfo.Append("<i>Title</i>: " + task.Title.Trim() + "; &nbsp");
                    if (task.DueDate != null)
                        addinfo.Append("<i>Due Date</i>: " + DateTime.Parse(task.DueDate.ToString()).ToString("MM/dd/yyyy") + "; &nbsp");
                    if (task.Responsible != null)
                        addinfo.Append("<i>Responsible</i>: " + JsonConvert.DeserializeObject<TaskPersonCommand>(task.Responsible).DisplayName + "; &nbsp");
                    addinfo.Append("</p>");

                    if (task.MeetingId > 0)
                        _topicService.PublishTopicHistoryAsync(AddTopicHistory(p.Comments, p.TaskId, (long)task.AgendaId, p.CreatedBy, updateflag, addinfo.ToString(), task.ClosureComment));
                });
            });

        }

        private TopicHistoryEvent AddTopicHistory(string description, long refId, long topicId, TaskPersonCommand createdBy, bool updateflag, string additionalInfo, string closureComment)
        {
            return new TopicHistoryEvent()
            {
                CategoryType = updateflag ? TopicHistoryStatus.TaskUpdated : TopicHistoryStatus.Taskcreated,
                Comments = description,
                GroupId = GroupType.TASK,
                ReferenceId = refId,
                TopicId = _meetingService.GetTopicId(topicId).GetAwaiter().GetResult(),
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now,
                AdditionalInfo = additionalInfo,
                TaskClosureComment = closureComment
            };
        }

        private string GetDescription(string taskDescription, string requestDescription)
        {
            if (taskDescription != null && requestDescription != null)
                return requestDescription != null ? requestDescription : taskDescription != null ? taskDescription : string.Empty;
            if (taskDescription != null && string.IsNullOrEmpty(requestDescription))
                return string.Empty;
            if (string.IsNullOrEmpty(taskDescription) && requestDescription != null)
                return requestDescription;

            return string.Empty;
        }

        private void AddTaskEmailNotification(EliteTask task, NotificationActionType actionType, TaskPersonCommand createdBy, TaskPersonCommand responsible,
            string taskTitle, long? TaskId, string description, DateTime? dueDate, long? committeeId, string committeeName, string role, string oldResponsible, bool isDemoUser,
            MeetingInfo meetingInfo, string poolIdEmailId, TaskPersonCommand modifiedBy, int? status, string committeePoolIdName, string closureComment = "", string rejectionComment = "", bool isCoResponsible = false, TaskPeople taskPeople = null)
        {
            //TODO: check for alternate option for validating Non-MB participants
            if (responsible.Uid.Length < 32)
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
                        TaskId = TaskId,
                        Description = description,
                        ClosureComment = closureComment,
                        RejectionComment = rejectionComment,
                        DueDate = dueDate,
                        CommitteeId = committeeId,
                        CommitteeName = committeeName,
                        Role = role,
                        OldResponsible = oldResponsible,
                        IsDemoUser = isDemoUser,
                        MeetingId = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingId)) ? Convert.ToInt64(meetingInfo.MeetingId) : 0,
                        MeetingName = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingName)) ? meetingInfo.MeetingName : string.Empty,
                        MeetingDatetime = (meetingInfo != null && !string.IsNullOrEmpty(meetingInfo.MeetingDate)) ? meetingInfo.MeetingDate : string.Empty,
                        IsConfidential = meetingInfo != null ? meetingInfo.IsConfidential : false,
                        PoolIdEmailId = poolIdEmailId,
                        ModifiedBy = modifiedBy,
                        status = status,
                        PoolIdName = committeePoolIdName,
                        TaskResponsible = taskResp,
                        TaskCoResponsible = taskCoResp,
                        topicTitleInEnglish = task.topicTitleInEnglish ?? string.Empty,
                        topicTitleInGerman = task.topicTitleInGerman ?? string.Empty,
                        isSubTask = isCoResponsible,
                        IsUpdate = taskPeople.IsUpdateOperation


                    },
                    GroupID = GroupType.TASK
                });
            }
        }

        private void CheckRoles(EliteTask task, List<TaskCommand> subTask)
        {
            if (this.rolesPermissions.UserRolesAndRights?.Count > 0)
            {
                RolePermissions rolePermissions = new RolePermissions();

                var roles = task.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(task.CommitteeId)) :
                                    this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId == null);
                var roleId = roles != null ? roles.RoleId : (int?)null;

                var permissionsActions = rolePermissions.GetUserAction(_configuration, task.CreatedBy.Upper(), task.Responsible.Upper(), task.CoResponsibles != null ? task.CoResponsibles.Upper() : "", subTask.Any(s => s.Responsible != null && s.Responsible.ToString().Upper().Contains(pUID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, task.MeetingId.HasValue, pUID);

                var subTaskPermissionsActions = subTask.Count > 0 ?
                    rolePermissions.GetUserSubTaskAction(_configuration, roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient,
                    subTask.Select(x => x.Responsible).ToList(), task.MeetingId.HasValue, pUID) : null;

                if (!(rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.CreateTask) ||
                    rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.EditTask) ||
                    rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.ChangeCompletedStatus) ||
                    rolePermissions.ValidateActionType(permissionsActions.ToList(), TaskEntityActionType.Status) ||
                    (subTaskPermissionsActions != null ?
                    (rolePermissions.ValidateActionType(subTaskPermissionsActions.ToList(), TaskEntityActionType.CreateTask) ||
                    rolePermissions.ValidateActionType(subTaskPermissionsActions.ToList(), TaskEntityActionType.EditTask) ||
                    rolePermissions.ValidateActionType(subTaskPermissionsActions.ToList(), TaskEntityActionType.ChangeCompletedStatus) ||
                    rolePermissions.ValidateActionType(subTaskPermissionsActions.ToList(), TaskEntityActionType.Status))
                    : true)))
                {
                    throw new EliteException($"unauthorized");
                }
            }
            else { throw new EliteException($"unauthorized"); }
        }
        private bool ShouldUpdateMainTask(TaskCommand request, EliteTask existingTask)
        {
            if (existingTask == null) return true;

            return
                existingTask.Title != request.Title ||
                existingTask.Description != request.Description ||
                existingTask.Status != request.Status ||
                existingTask.DueDate != request.DueDate ||
                existingTask.FileLink != request.FileLink ||
                existingTask.CommitteeId != request.CommitteeId ||
                existingTask.IsNotify != request.notifyUsers ||
                existingTask.MeetingId != request.MeetingID ||
                existingTask.MeetingDate != request.MeetingDate ||
                existingTask.AgendaId != request.AgendaID ||
                existingTask.MeetingStatus != request.MeetingStatus ||
                existingTask.Responsible != JsonConvert.SerializeObject(request.Responsible) ||
                existingTask.CoResponsibles != JsonConvert.SerializeObject(request.CoResponsible) ||
                existingTask.ResponsibleEmailRecipient != JsonConvert.SerializeObject(request.ResponsibleEmailRecipient) ||
                existingTask.IsCustomEmailRecipient != request.IsCustomEmailRecipient ||
                existingTask.JiraTicketInfo != JsonConvert.SerializeObject(request.JiraTicketInfo) ||
                existingTask.ResponsibleDivision != JsonConvert.SerializeObject(request.ResponsibleDivision);
        }

    }

    public class ProtocolTask
    {

        public enum TaskType
        {
            MainTask,
            SubTask
        }
        public long TaskId { get; set; }
        public long? TopicId { get; set; }
        public Guid TaskGuid { get; set; }
        public TaskType TaskCategory { get; set; }
        public string Comments { get; set; }

        public TaskPersonCommand CreatedBy { get; set; }

        public bool IsDeleted { get; set; }

    }
}
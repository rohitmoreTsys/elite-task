using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using Elite.Task.Microservice.CommonLib;
using Elite.Task.Microservice.Models.Entities;
using Elite.Task.Microservice.NotificationServices;
using Elite.Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Elite.Common.Utilities.Encription;
using static System.Net.WebRequestMethods;

namespace Elite.Common.Utilities.NotificationServices
{
    public class EmailNotificationCommandHandler : IRequestHandler<EmailNotificationCommand, long>
    {
        private readonly HttpClient _httpClient;
        protected readonly IRepositoryEventStore _repositoryEventStore;
        protected readonly IConfiguration _configuration;
        protected readonly IRequestContext _requestContext;
        private readonly List<SendEmail> _sendMailsList;
        readonly AsyncRetryPolicy<HttpResponseMessage> _httpRequestPolicy;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;
        private IHostingEnvironment _env;
        private readonly ILogger<EmailNotificationCommandHandler> _logger;
        private readonly IHTMLReadUtility _readHTML;
        public string filename;
        string poolIdEmailId;
        string poolIdName;
        List<UserDeputies> restrictedUserDeputies;

        #region Const for meeting Email
        private const string EMAILNOTIFICATIONSTATUS = "EmailNotification:TokenReplacement:STATUS";
        private const string EMAILNOTIFICATIONPERSON = "EmailNotification:TokenReplacement:PERSON";
        private const string EMAILNOTIFICATIONTASKLINK = "EmailNotification:TokenReplacement:TASKLINK";
        private const string EMAILNOTIFICATIONTASK = "EmailNotification:TokenReplacement:TASK";
        private const string EMAILNOTIFICATIONDEMO = "EmailNotification:TokenReplacement:DEMO";
        private const string EMAILNOTIFICATIONDUEDATE = "EmailNotification:TokenReplacement:DUEDATE";
        private const string EMAILNOTIFICATIONCOMMITTEE = "EmailNotification:TokenReplacement:COMMITTEE";
        private const string EMAILNOTIFICATIONROLE = "EmailNotification:TokenReplacement:ROLE";
        private const string EMAILNOTIFICATIONTITLE = "EmailNotification:TokenReplacement:TITLE";
        private const string EMAILNOTIFICATIONTO = "EmailNotification:TokenReplacement:TO";
        private const string EMAILNOTIFICATIONMEETINGLINK = "EmailNotification:TokenReplacement:MEETINGLINK";
        private const string EMAILNOTIFICATIONTASKDESCRIPTION = "EmailNotification:TokenReplacement:TASKDESCRIPTION";
        private const string EMAILNOTIFICATIONCLOSURECOMMENT = "EmailNotification:TokenReplacement:CLOSURECOMMENT";
        private const string EMAILNOTIFICATIONREJECTIONCOMMENT = "EmailNotification:TokenReplacement:REJECTIONCOMMENT";
        private const string EMAILNOTIFICATIONMEETINGNAME = "EmailNotification:TokenReplacement:MEETINGNAME";
        private const string EMAILNOTIFICATIONMEETINGACTIONLINK = "EmailNotification:MeetingActionLink";
        private const string EMAILNOTIFICATIONTASKACTIONLINK = "EmailNotification:TaskActionLink";
        private const string EMAILNOTIFICATIONMEETINGDATE = "EmailNotification:TokenReplacement:MEETINGDATE";
        private const string EMAILNOTIFICATIONMEETINGINFO = "EmailNotification:TokenReplacement:MEETINGINFO";
        private const string CREATOREMAIL = "EmailNotification:TokenReplacement:CREATOREMAIL";
        private const string DEMOCONTENTGERMAN = "EmailNotification:TokenReplacement:DEMOCONTENTGERMAN";
        private const string TASKTITLEGERMAN = "EmailNotification:TokenReplacement:TASKTITLEGERMAN";
        private const string TASKDESCRIPTIONGERMAN = "EmailNotification:TokenReplacement:TASKDESCRIPTIONGERMAN";
        private const string DUEDATEGERMAN = "EmailNotification:TokenReplacement:DUEDATEGERMAN";
        private const string CREATORGERMAN = "EmailNotification:TokenReplacement:CREATORGERMAN";
        private const string CREATOREMAILGERMAN = "EmailNotification:TokenReplacement:CREATOREMAILGERMAN";
        private const string TASKACTIONLINKGERMAN = "EmailNotification:TokenReplacement:TASKACTIONLINKGERMAN";
        private const string MEETINGHEADER = "EmailNotification:TokenReplacement:MEETINGHEADER";
        private const string MEETINGDATA = "EmailNotification:TokenReplacement:MEETINGDATA";
        private const string MEETINGLINKINFO = "EmailNotification:TokenReplacement:MEETINGLINKINFO";
        private const string CONFIDENTIAL = "EmailNotification:TokenReplacement:CONFIDENTIAL";
        private const string MEETINGNAMEGERMAN = "EmailNotification:TokenReplacement:MEETINGNAMEGERMAN";
        private const string MEETINGDATEGERMAN = "EmailNotification:TokenReplacement:MEETINGDATEGERMAN";
        private const string MEETINGACTIONLINKGERMAN = "EmailNotification:TokenReplacement:MEETINGACTIONLINKGERMAN";
        private const string MEETINGHEADERGERMAN = "EmailNotification:TokenReplacement:MEETINGHEADERGERMAN";
        private const string MEETINGDATAGERMAN = "EmailNotification:TokenReplacement:MEETINGDATAGERMAN";
        private const string MEETINGLINKINFOGERMAN = "EmailNotification:TokenReplacement:MEETINGLINKINFOGERMAN";
        private const string CONFIDENTIALGERMAN = "EmailNotification:TokenReplacement:CONFIDENTIALGERMAN";
        private const string RESPONSIBLEGERMAN = "EmailNotification:TokenReplacement:RESPONSIBLEGERMAN";
        private const string TASKRESPONSIBLE = "EmailNotification:TokenReplacement:TASKRESPONSIBLE";
        private const string TASKCORESPONSIBLE = "EmailNotification:TokenReplacement:TASKCORESPONSIBLE";
        private const string TOPICTITLEINENGLISH = "EmailNotification:TokenReplacement:TOPICTITLEINENGLISH";
        private const string TOPICTITLEGERMAN = "EmailNotification:TokenReplacement:TOPICTITLEGERMAN";
        private const string TASKHEADER = "#TASKHEADER#";
        private const string TASKADDITIONALDATA = "EmailNotification:TokenReplacement:TASKADDITIONALDATA";
        private const string TASKADDITIONALDATAGERMAN = "EmailNotification:TokenReplacement:TASKADDITIONALDATAGERMAN";
        private const string TASKADDITIONALHEADER = "EmailNotification:TokenReplacement:TASKADDITIONALHEADER";
        private const string TASKADDITIONALHEADERGERMAN = "EmailNotification:TokenReplacement:TASKADDITIONALHEADERGERMAN";
        private const string TASKUPDATEDBY = "EmailNotification:TokenReplacement:TASKUPDATEDBY";
        #endregion

        public EmailNotificationCommandHandler(IHostingEnvironment env, IRepositoryEventStore repositoryEventStore, IConfiguration configuration, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory, ILogger<EmailNotificationCommandHandler> logger, IHTMLReadUtility readHTML, IRequestContext requestContext)
        {
            _logger = logger;
            _userServiceFactory = userServiceFactory;
            this._configuration = configuration;
            this._requestContext = requestContext;
            _userService = userServiceFactory(_configuration, _requestContext);
            this._repositoryEventStore = repositoryEventStore;
            this._sendMailsList = new List<SendEmail>();
            _env = env;
            _httpClient = new HttpClient();
            _httpRequestPolicy = Policy.HandleResult<HttpResponseMessage>(
                 r => r.StatusCode == HttpStatusCode.InternalServerError)
             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
            _readHTML = readHTML;

        }


        public async System.Threading.Tasks.Task<long> Handle(EmailNotificationCommand request, CancellationToken cancellationToken)
        {
            await ProcessNotification(request);
            return request.Id;
        }


        private async System.Threading.Tasks.Task ProcessNotification(EmailNotificationCommand request)
        {
            NoticationEventStore notification = null;

            try
            {

                notification = await _repositoryEventStore.GetById(request.Id);
                if (notification != null)
                {
                    var message = JsonConvert.DeserializeObject<TaskEvent>(notification.JsonMessage);
                    poolIdEmailId = message.PoolIdEmailId;
                    poolIdName = message.PoolIdName;

                    restrictedUserDeputies = await _userService.GetRestrictedUserDeputies(message?.Responsible?.Uid);

                    await ProcessNotification(new EmailNotificationEvent()
                    {
                        Message = JsonConvert.DeserializeObject<TaskEvent>(notification.JsonMessage),
                        ActionType = (NotificationActionType)notification.ActionType,
                        TaskId = notification.Sourcetypeid
                    });

                    bool isMailSend = false;
                    if (this._sendMailsList?.Count == 0)
                    {
                        notification.IsFailed = true;
                        notification.IsProcessed = false;
                        notification.FailureReason = "No email IDs found.";
                        _repositoryEventStore.Update(notification);
                        await _repositoryEventStore.UnitOfWork.SaveEntitiesAsync();
                    }
                    if (this._sendMailsList?.Count > 0)
                    {
                        if (string.IsNullOrEmpty(poolIdEmailId))
                        {
                            isMailSend = await SendEmail(this._sendMailsList);
                        }
                        else
                        {
                            isMailSend = await SendTaskEmailPoolId(this._sendMailsList, poolIdEmailId, poolIdName);
                        }
                    }

                    if (isMailSend)
                    {
                        notification.IsProcessed = true;
                        notification.IsFailed = false;
                        notification.ProcessedDate = DateTime.Now;
                        _repositoryEventStore.Update(notification);
                        await _repositoryEventStore.UnitOfWork.SaveEntitiesAsync();
                    }
                    else
                    {
                        notification.IsFailed = true;
                        notification.IsProcessed = false;
                        _repositoryEventStore.Update(notification);
                        await _repositoryEventStore.UnitOfWork.SaveEntitiesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($" {nameof(EmailNotificationCommandHandler)} ProcessNotification failed : NotificationId  {0}  ", notification != null ? notification.Id : 0);
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
            }
        }

        private async System.Threading.Tasks.Task ProcessNotification(BaseNotificationEvent notification)
        {
            var notificationEvent = (EmailNotificationEvent)notification;

            switch (notification.ActionType)
            {
                case NotificationActionType.CREATOR_COMPLETE_TO_INPROGRESS:
                case NotificationActionType.CREATOR_COMPLETE_TO_OPEN:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_REOPEN);
                    //send mail to Responsible  for status with creator details in mail
                    break;
                case NotificationActionType.COMPLETE_TO_INPROGRESS:
                case NotificationActionType.COMPLETE_TO_OPEN:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_REOPEN_RESPONSIBLE);
                    //send mail to Responsible  for status with creator details in mail
                    break;
                case NotificationActionType.NOTIFY_OLD_RESPONSIBLE:
                    //NOTIFY OLD RESPONSIBLE
                    await SendEmailTask(notification, SendEmailTaskType.TASK_RESPONSIBLE_CHANGE);
                    break;
                case NotificationActionType.ASSIGN_TO_RESPONSIBLE:
                    //send mail to Responsible  
                    await SendEmailTask(notification, SendEmailTaskType.TASK_RESPONSIBLE);

                    break;
                case NotificationActionType.NOTIFY_OLD_CO_RESPONSIBLE:
                    //NOTIFY OLD RESPONSIBLE
                    await SendEmailTask(notification, SendEmailTaskType.TASK_CO_RESPONSIBLE_CHANGE);
                    break;
                case NotificationActionType.ASSIGN_TO_CO_RESPONSIBLE:
                    //send mail to Responsible  
                    await SendEmailTask(notification, SendEmailTaskType.TASK_CO_RESPONSIBLE);
                    break;

                case NotificationActionType.CREATOR_INPROGRESS_TO_COMPLETE:
                case NotificationActionType.CREATOR_OPEN_TO_COMPLETE:
                case NotificationActionType.INPROGRESS_TO_COMPLETE:
                case NotificationActionType.OPEN_TO_COMPLETE:
                    //send mail to creator for status
                    await SendEmailTask(notification, SendEmailTaskType.TASK_COMPLETION);
                    break;
                case NotificationActionType.REJECT_COMPLETE_INPROGRESS:
                    await SendEmailTask(notification, SendEmailTaskType.REJECT_COMPLETE_INPROGRESS);
                    break;
                case NotificationActionType.TASK_TO_DELETE:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_DELETE);
                    break;
                case NotificationActionType.SUBTASK_TO_DELETE:
                    await SendEmailTask(notification, SendEmailTaskType.SUBTASK_TASK_DELETE);
                    break;
                case NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_MEETING_RESPONSIBLE);
                    break;
                case NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_MEETING_CO_RESPONSIBLE);
                    break;
                case NotificationActionType.FROM_MEETING_ASSIGN_TO_RESPONSIBLE_CORPORATE:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_MEETING_RESPONSIBLE_CORPORATE);
                    break;
                case NotificationActionType.FROM_MEETING_ASSIGN_TO_CO_RESPONSIBLE_CORPORATE:
                    await SendEmailTask(notification, SendEmailTaskType.TASK_MEETING_CO_RESPONSIBLE_CORPORATE);
                    break;
                default: throw new EliteException("Invalid Notification ActionType");

            }
        }

        private async System.Threading.Tasks.Task SendEmailTask(BaseNotificationEvent notification, SendEmailTaskType sendEmailTaskType)
        {
            const string COMMITTEECOMMONTEXT = "Committee/Gremium";
            const string COMMITTEECOMMONTEXTBOM = "Committee";
            var taskEvent = notification as EmailNotificationEvent;
            var message = ((TaskEvent)taskEvent.Message);

            if (taskEvent == null || taskEvent.Message == null)
                return;

            //Somtimes message is null. 
            if (message == null)
                return;

            message.TaskId = taskEvent.TaskId;
            SendEmail sendEmail = new SendEmail();
            sendEmail.basePath = _configuration.GetSection("BasePathTemplate").Value;
            sendEmail.SMTPEmailwithTemplate = SendEmailType.TASK;
            string status = "";

            var demoUser = message.IsDemoUser;

            string creatorEmailId = string.Empty;
            if (message.CreatedBy != null)
            {
                var createdByUserEmail = await _userService.GetUserEmailId(message.CreatedBy.Uid);
                creatorEmailId = createdByUserEmail?.Email ?? string.Empty;
            }
            List<string> emailIds = new List<string>();
            //Check for restriced BoM member deputies
            emailIds = GetRestrictedDeputyEmailIds(message.CommitteeId);

            //If restricted BoM member deputies are not there then send email to responsible
            if (emailIds.Count <= 0)
            {
                var emailId = await _userService.GetUserEmailId(message?.Responsible?.Uid);
                if (emailId != null && !string.IsNullOrWhiteSpace(emailId.Email))
                    emailIds.Add(emailId.Email);
            }

            if (sendEmailTaskType == SendEmailTaskType.TASK_RESPONSIBLE)
            {
                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                    {
                        if (message.isSubTask)
                        {
                            sendEmail.Subject = message.IsUpdate
                                ? _configuration.GetValue<string>("EmailNotification:SubTaskUpdate:Subject")
                                : _configuration.GetValue<string>("EmailNotification:SubTaskResponsible:Subject");
                        }
                        else
                        {
                            sendEmail.Subject = message.IsUpdate
                                ? _configuration.GetValue<string>("EmailNotification:TaskUpdate:Subject")
                                : _configuration.GetValue<string>("EmailNotification:TaskResponsible:Subject");
                        }
                    }
                    else
                    {
                        sendEmail.Subject = message.isSubTask
                            ? _configuration.GetValue<string>("EmailNotification:SubTaskResponsible:Trial-Subject")
                            : _configuration.GetValue<string>("EmailNotification:TaskResponsible:Trial-Subject");
                    }

                    if (message != null && message.MeetingId != null && message.MeetingId > 0)
                    {
                        var meetingName = ConcadenatedMeetingName(message.MeetingName);
                        var MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);
                        if (message.IsUpdate)
                        {
                            sendEmail.Body = TokenReplacement(_readHTML.TaskUpdate,
                            message.OldResponsible,
                            message.CreatedBy.DisplayName,
                            _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                            message.TaskTitle, message.Role,
                            message.CommitteeName, status,
                            message.IsDemoUser,
                            message.DueDate,
                            message.TaskId, message.Description,
                            message.ClosureComment,
                            message.RejectionComment,
                            meetingName, MeetingDate,
                            _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                            (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                            message.TaskResponsible ?? string.Empty,
                            message.TaskCoResponsible ?? string.Empty,
                            message.topicTitleInEnglish ?? string.Empty,
                            message.topicTitleInGerman ?? string.Empty,
                            message.isSubTask,
                            message.ModifiedBy?.DisplayName ?? string.Empty);
                        }
                        else
                        {
                            sendEmail.Body = TokenReplacement(_readHTML.MeetingTaskResponsible,
                            message.OldResponsible,
                            message.CreatedBy.DisplayName,
                            _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                            message.TaskTitle, message.Role,
                            message.CommitteeName, status,
                            message.IsDemoUser,
                            message.DueDate,
                            message.TaskId, message.Description,
                            message.ClosureComment,
                            message.RejectionComment,
                            meetingName, MeetingDate,
                            _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                            (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                            message.TaskResponsible ?? string.Empty,
                            message.TaskCoResponsible ?? string.Empty,
                            message.topicTitleInEnglish ?? string.Empty,
                            message.topicTitleInGerman ?? string.Empty,
                            message.isSubTask,
                            message.ModifiedBy?.DisplayName ?? string.Empty);
                        }

                    }
                    else
                    {
                        if (message.IsUpdate)
                        {
                            sendEmail.Body = TokenReplacement(_readHTML.TaskUpdate, message.Responsible.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                        }
                        else
                        {
                            sendEmail.Body = TokenReplacement(_readHTML.TaskResponsible, message.Responsible.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                        }
                    }
                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_CO_RESPONSIBLE)
            {
                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                    {
                        if (message.IsUpdate)
                        {
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskUpdate:Subject").Value;
                        }
                        else
                        {
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCoResponsible:Subject").Value;
                        }
                    }
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCoResponsible:Trial-Subject").Value;

                    message.OldResponsible = message.OldResponsible != null ? message.OldResponsible : string.Empty;
                    if (message.IsUpdate)
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskUpdate, message.OldResponsible, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                    }
                    else
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskCoResponsible, message.OldResponsible, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                    }
                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_RESPONSIBLE_CHANGE)
            {
                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskResponsibleChanged:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskResponsibleChanged:Trial-Subject").Value;
                    
                    sendEmail.Body = TokenReplacement(_readHTML.TaskResponsibleChanged, message.OldResponsible, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                                    message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 3;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_CO_RESPONSIBLE_CHANGE)
            {

                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCoResponsibleChanged:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCoResponsibleChanged:Trial-Subject").Value;


                    sendEmail.Body = TokenReplacement(_readHTML.TaskCoResponsibleChanged, message.OldResponsible, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);
                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 3;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_MEETING_RESPONSIBLE)
            {
                string meetingName = string.Empty, MeetingDate = string.Empty;

                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingResponsible:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingResponsible:Trial-Subject").Value;

                    meetingName = ConcadenatedMeetingName(message.MeetingName);
                    MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);

                    sendEmail.Body = TokenReplacement(_readHTML.MeetingTaskResponsible,
                        message.OldResponsible,
                        message.CreatedBy.DisplayName,
                        _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                        message.TaskTitle, message.Role,
                        message.CommitteeName, status,
                        message.IsDemoUser,
                        message.DueDate,
                        message.TaskId, message.Description, message.ClosureComment, message.RejectionComment,
                        meetingName, MeetingDate,
                        _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                        (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);

                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_MEETING_CO_RESPONSIBLE)
            {
                string meetingName = string.Empty, MeetingDate = string.Empty;

                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsible:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsible:Trial-Subject").Value;


                    meetingName = ConcadenatedMeetingName(message.MeetingName);
                    MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);

                    sendEmail.Body = TokenReplacement(_readHTML.MeetingTaskCoResponsible,
                                                message.OldResponsible,
                                                message.CreatedBy.DisplayName,
                                                _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                                                message.TaskTitle, message.Role,
                                                message.CommitteeName, status,
                                                message.IsDemoUser,
                                                message.DueDate,
                                                message.TaskId, message.Description, message.ClosureComment, message.RejectionComment,
                                                meetingName, MeetingDate,
                                                _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                                                (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                                                message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);

                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_MEETING_RESPONSIBLE_CORPORATE)
            {
                string meetingName = string.Empty, MeetingDate = string.Empty;

                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingResponsibleBoM:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingResponsibleBoM:Trial-Subject").Value;

                    meetingName = ConcadenatedMeetingName(message.MeetingName);
                    MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);

                    sendEmail.Body = TokenReplacement(_readHTML.MeetingTaskResponsibleBoM,
                        message.OldResponsible,
                        message.CreatedBy.DisplayName,
                        _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                        message.TaskTitle, message.Role,
                        message.CommitteeName, status,
                        message.IsDemoUser,
                        message.DueDate,
                        message.TaskId, message.Description, message.ClosureComment, message.RejectionComment,
                        meetingName, MeetingDate,
                        _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                        (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);

                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_MEETING_CO_RESPONSIBLE_CORPORATE)
            {
                string meetingName = string.Empty, MeetingDate = string.Empty;

                if (emailIds.Count > 0)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsible:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskForMeetingCoResponsible:Trial-Subject").Value;


                    meetingName = ConcadenatedMeetingName(message.MeetingName);
                    MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);

                    sendEmail.Body = TokenReplacement(_readHTML.MeetingTaskCoResponsibleBoM,
                                                message.OldResponsible,
                                                message.CreatedBy.DisplayName,
                                                _configuration.GetSection(EMAILNOTIFICATIONTASKACTIONLINK).Value,
                                                message.TaskTitle, message.Role,
                                                message.CommitteeName, status,
                                                message.IsDemoUser,
                                                message.DueDate,
                                                message.TaskId, message.Description, message.ClosureComment, message.RejectionComment,
                                                meetingName, MeetingDate,
                                                _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                                                (long)message.MeetingId, message.IsConfidential, creatorEmailId,
                                                message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);

                    foreach (var email in emailIds)
                    {
                        AddRecipient(sendEmail, email);
                    }
                    sendEmail.taskType = 1;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_REOPEN_RESPONSIBLE)
            {
                var emailId = await _userService.GetUserEmailId(message.CreatedBy.Uid);

                status = "ReOpened";

                if (emailId != null && !string.IsNullOrWhiteSpace(emailId?.Email))
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Trial-Subject").Value;


                    sendEmail.Body = TokenReplacement(_readHTML.TaskStatus, message.ModifiedBy.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                                    message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);


                    if (message.status != 15)
                        AddRecipient(sendEmail, emailId.Email);

                    if (emailIds.Count > 0)
                    {
                        foreach (var email in emailIds)
                        {
                            AddRecipient(sendEmail, email);
                        }
                    }
                    sendEmail.taskType = 4;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_REOPEN)
            {
                var emailId = await _userService.GetUserEmailId(message.CreatedBy.Uid);

                status = "ReOpened";

                if (emailId != null && !string.IsNullOrWhiteSpace(emailId?.Email))
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Trial-Subject").Value;


                    sendEmail.Body = TokenReplacement(_readHTML.TaskStatus, message.ModifiedBy.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment,
                        message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);

                    if (message.status != 15)
                        AddRecipient(sendEmail, emailId.Email);

                    if (emailIds.Count > 0)
                    {
                        foreach (var email in emailIds)
                        {
                            AddRecipient(sendEmail, email);
                        }
                    }
                    sendEmail.taskType = 4;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_COMPLETION || sendEmailTaskType == SendEmailTaskType.REJECT_COMPLETE_INPROGRESS)
            {
                long meetingId = 0;
                string meetingName = string.Empty, MeetingDate = string.Empty;
                var emailId = await _userService.GetUserEmailId(message.CreatedBy.Uid);
                if (sendEmailTaskType == SendEmailTaskType.TASK_COMPLETION)
                    status = "Completed";
                else
                    status = "InProgress";
                // send email to the creator and responsible also when task completed
                if (emailId != null && !string.IsNullOrWhiteSpace(emailId?.Email))
                {
                    if (sendEmailTaskType == SendEmailTaskType.TASK_COMPLETION)
                    {
                        if (!demoUser)
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCompletion:Subject").Value;
                        else
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskCompletion:Trial-Subject").Value;

                    }
                    else
                    {
                        if (!demoUser)
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Subject").Value;
                        else
                            sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskStatus:Trial-Subject").Value;

                    }
                    meetingName = ConcadenatedMeetingName(message.MeetingName);
                    MeetingDate = ConcadenatedMeetingDate(message.MeetingDatetime);
                    if (message.MeetingId != null)
                        meetingId = (long)message.MeetingId;

                    if (sendEmailTaskType == SendEmailTaskType.TASK_COMPLETION)
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskCompleted, message?.ModifiedBy?.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, message.ClosureComment, message.RejectionComment, meetingName, MeetingDate,
                        _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                       meetingId, message.IsConfidential, creatorEmailId, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);
                        if (message.CreatedBy.Uid.ToUpper() == message.Responsible.Uid.ToUpper())
                            sendEmail.Body = sendEmail.Body.Replace("This is to inform you that status of the task assigned to you in eLite is changed.", "This is to inform you that status of the task assigned by you in eLite is changed.");
                    }

                    else
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskRejected, message?.ModifiedBy?.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, message.ClosureComment, message.RejectionComment, meetingName, MeetingDate,
                        _configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value,
                       meetingId, message.IsConfidential, creatorEmailId, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask);
                        if (message.CreatedBy.Uid.ToUpper() == message.Responsible.Uid.ToUpper())
                            sendEmail.Body = sendEmail.Body.Replace("This is to inform you that status of the task assigned to you in eLite is changed.", "This is to inform you that status of the task assigned by you in eLite is changed.");
                    }

                    if (emailIds.Count > 0)
                    {
                        foreach (var email in emailIds)
                        {
                            AddRecipient(sendEmail, email);
                        }
                    }
                    if (message.Responsible != null)
                    {
                        var emailIdResponsible = await _userService.GetUserEmailId(message.Responsible.Uid);
                        if (emailIdResponsible != null && !string.IsNullOrWhiteSpace(emailIdResponsible.Email))
                        {
                            AddRecipient(sendEmail, emailIdResponsible.Email);
                        }
                    }
                    else
                    {
                        sendEmail.Receipients = message.CommitteeManagerEmailList;
                    }
                    sendEmail.taskType = 4;
                    this._sendMailsList.Add(sendEmail);
                }
            }
            else if (sendEmailTaskType == SendEmailTaskType.TASK_DELETE || sendEmailTaskType == SendEmailTaskType.SUBTASK_TASK_DELETE)
            {
                var emailId = await _userService.GetUserEmailId(message.CreatedBy.Uid);
                sendEmail.taskType = 2;
                if (emailId != null)
                {
                    if (!demoUser)
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskDelete:Subject").Value;
                    else
                        sendEmail.Subject = _configuration.GetSection("EmailNotification:TaskDelete:Trial-Subject").Value;



                    if (sendEmailTaskType == SendEmailTaskType.SUBTASK_TASK_DELETE)
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskDelete, message.Responsible.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, (int?)SendEmailTaskType.SUBTASK_TASK_DELETE, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);

                    }
                    else
                    {
                        sendEmail.Body = TokenReplacement(_readHTML.TaskDelete, message.Responsible.DisplayName, message.CreatedBy.DisplayName, _configuration.GetSection("EmailNotification:TaskActionLink").Value, message.TaskTitle, message.Role, message.CommitteeName, status, message.IsDemoUser, message.DueDate, message.TaskId, message.Description, creatorEmailId, message.ClosureComment, message.RejectionComment, message.TaskResponsible ?? string.Empty, message.TaskCoResponsible ?? string.Empty, message.topicTitleInEnglish ?? string.Empty, message.topicTitleInGerman ?? string.Empty, message.isSubTask, message.ModifiedBy?.DisplayName ?? string.Empty);

                    }

                    if (emailIds.Count > 0)
                    {
                        foreach (var email in emailIds)
                        {
                            AddRecipient(sendEmail, email);
                        }
                    }
                    this._sendMailsList.Add(sendEmail);
                }
            }

            if (sendEmailTaskType == SendEmailTaskType.TASK_MEETING_RESPONSIBLE_CORPORATE)
            {
                //to show the committee name in the subject.
                if (!string.IsNullOrEmpty(message.CommitteeName))
                    sendEmail.Subject = String.Format(sendEmail.Subject, message.CommitteeName + ":");
                else
                    sendEmail.Subject = String.Format(sendEmail.Subject, string.Empty);
            }
            else
            {
                //to show the committee name in the subject.
                if (!string.IsNullOrEmpty(message.CommitteeName))
                {
                    sendEmail.Subject = string.Format(sendEmail.Subject ?? string.Empty, message.CommitteeName + ":");
                }
                else
                {
                    sendEmail.Subject = string.Format(sendEmail.Subject ?? string.Empty, string.Empty);
                }
            }

        }
        /// <summary>
        /// Get Restricted BoM member Deputies EmailIds
        /// </summary>
        /// <param name="committeeId"></param>
        /// <returns></returns>
        private List<string> GetRestrictedDeputyEmailIds(long? committeeId)
        {
            List<string> emailIds = new List<string>();
            if (restrictedUserDeputies != null)
            {
                if (committeeId != null)
                {
                    var committeeUserDeputies = restrictedUserDeputies.Where(x => x.CommitteeId.Equals(committeeId)).ToList();
                    if (committeeUserDeputies.Count > 0)
                    {
                        emailIds.AddRange(committeeUserDeputies.Select(x => x.EmailId).Distinct());
                    }
                }
                else
                {
                    emailIds.AddRange(restrictedUserDeputies.Select(x => x.EmailId).Distinct());
                }
            }
            return emailIds;
        }

        /// <summary>
        /// Send email for task using the committee pool id
        /// </summary>
        /// <param name="sendEmail"></param>
        /// <param name="poolIdEmailId"></param>
        /// <returns></returns>
        private async Task<bool> SendTaskEmailPoolId(List<SendEmail> sendEmail, string poolIdEmailId, string poolIdName)
        {
            try
            {
                SendTaskEmailPoolId sendTaskEmailPoolId = new SendTaskEmailPoolId();
                sendTaskEmailPoolId.PoolIdEmailId = poolIdEmailId;
                sendTaskEmailPoolId.PoolIdName = poolIdName;
                sendTaskEmailPoolId.SMPTPMailList = sendEmail;

                var outLookRequest = JsonConvert.SerializeObject(sendTaskEmailPoolId);
                var stringContent = new StringContent(outLookRequest, UnicodeEncoding.UTF8, "application/json");

                var taskPostResponse = await _httpRequestPolicy.ExecuteAsync(async () => await _httpClient.PostAsync(_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPPoolIdEmail/", stringContent));

                if ((int)taskPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
                    throw new EliteException($" Api call was failed {_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPPoolIdEmail/"}  with status code - {((int)taskPostResponse.StatusCode)} ");

                return true;
            }
            catch (EliteException ex)
            {
                _logger.LogError($" {nameof(EmailNotificationCommandHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($" {nameof(EmailNotificationCommandHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
                return false;
            }
        }

        private async Task<bool> SendEmail(List<SendEmail> sendEmail)
        {
            try
            {
                var outLookRequest = JsonConvert.SerializeObject(sendEmail);
                var stringContent = new StringContent(outLookRequest, UnicodeEncoding.UTF8, "application/json");
                var topicPostResponse = await _httpRequestPolicy.ExecuteAsync(async () => await _httpClient.PostAsync(_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPEmail/", stringContent));

                //topicPostResponse = await _httpClient.PostAsync(_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPEmail/", stringContent);

                if ((int)topicPostResponse.StatusCode != (int)System.Net.HttpStatusCode.OK)
                    throw new EliteException($" Api call was failed {_configuration.GetSection("EmailNotification:outlookapi").Value + "/SMTPEmail/"}  with status code - {((int)topicPostResponse.StatusCode)} ");

                return true;
            }
            catch (EliteException ex)
            {
                _logger.LogError($" {nameof(EmailNotificationCommandHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($" {nameof(EmailNotificationCommandHandler)}  SendEmail failed   ");
                _logger.LogError(ExceptionFormator.FormatExceptionMessage(ex));
                return false;
            }
        }

        private enum SendEmailTaskType
        {
            TASK_RESPONSIBLE,
            TASK_REOPEN,
            TASK_COMPLETION,
            TASK_DELETE,
            SUBTASK_TASK_DELETE,
            TASK_RESPONSIBLE_CHANGE,
            TASK_REOPEN_RESPONSIBLE,
            TASK_CO_RESPONSIBLE,
            TASK_CO_RESPONSIBLE_CHANGE,
            TASK_MEETING_RESPONSIBLE,
            TASK_MEETING_CO_RESPONSIBLE,
            TASK_MEETING_RESPONSIBLE_CORPORATE,
            TASK_MEETING_CO_RESPONSIBLE_CORPORATE,
            REJECT_COMPLETE_INPROGRESS
        }


        private string TokenReplacement(string source, string to, string person, string link, string title, string role, string committee, string status, bool demouser, DateTime? duedate, long? task, string taskDescription, string emailAddress, string closureComment, string rejectionComment, string taskResponsible, string taskCoResponsible, string topicTitleInEnglish, string topicTitleInGerman, bool? isSubTask, string modifiedBy = null)
        {
            string notificationLink = link + "?id=" + task;
            string demoEmailContent = string.Empty;
            string closureCommentElement = string.Empty;
            string rejectionCommentElement = string.Empty;
            if (!string.IsNullOrEmpty(closureComment))
                closureCommentElement = ClosureCommentElement(closureComment);
            if (!string.IsNullOrEmpty(rejectionComment))
                rejectionCommentElement = RejectionCommentElement(rejectionComment);
            if (demouser)
            {
                demoEmailContent = GetDemoContent();
            }
            string taskAdditionalHeader = InsertTaskAdditionalHeader(0, false, isSubTask ?? false);
            string taskAdditionalHeaderGerman = InsertTaskAdditionalHeader(0, true, isSubTask ?? false);
            string taskAdditonalData = InsertTaskAdditionalData(taskResponsible ?? string.Empty, taskCoResponsible ?? string.Empty, topicTitleInEnglish ?? string.Empty, 0, isSubTask ?? false);
            string taskAdditonalDataGerman = InsertTaskAdditionalDataGerman(taskResponsible ?? string.Empty, taskCoResponsible ?? string.Empty, topicTitleInGerman ?? string.Empty, 0, isSubTask ?? false);

            try
            {
                var statusLink = _configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value;
                var returnResult = source.Replace(_configuration.GetSection(EMAILNOTIFICATIONTO).Value, to)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONPERSON).Value, person)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASKLINK).Value, notificationLink)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTITLE).Value, title)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONROLE).Value, role)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCOMMITTEE).Value, committee)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONDUEDATE).Value, duedate.HasValue ? duedate.Value.ToString("dd.MM.yyyy") : "")
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value, status)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONDEMO).Value, demoEmailContent)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASK).Value, task == (int?)SendEmailTaskType.SUBTASK_TASK_DELETE ? "Sub-Task" : "Task")
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASKDESCRIPTION).Value, taskDescription)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCLOSURECOMMENT).Value, closureCommentElement)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONREJECTIONCOMMENT).Value, rejectionCommentElement)
                .Replace(_configuration.GetSection(CREATOREMAIL).Value, emailAddress)
                .Replace(_configuration.GetSection(DEMOCONTENTGERMAN).Value, demoEmailContent)
                .Replace(_configuration.GetSection(TASKTITLEGERMAN).Value, title)
                .Replace(_configuration.GetSection(TASKDESCRIPTIONGERMAN).Value, taskDescription)
                .Replace(_configuration.GetSection(DUEDATEGERMAN).Value, duedate.HasValue ? duedate.Value.ToString("dd.MM.yyyy") : "")
                .Replace(_configuration.GetSection(CREATORGERMAN).Value, person)
                .Replace(_configuration.GetSection(CREATOREMAILGERMAN).Value, emailAddress)
                .Replace(_configuration.GetSection(TASKACTIONLINKGERMAN).Value, notificationLink)
                .Replace(_configuration.GetSection(TASKRESPONSIBLE).Value, taskResponsible ?? to ?? string.Empty)
                .Replace(_configuration.GetSection(TASKCORESPONSIBLE).Value, taskCoResponsible ?? string.Empty)
                .Replace(_configuration.GetSection(TOPICTITLEINENGLISH).Value, topicTitleInEnglish ?? string.Empty)
                .Replace(_configuration.GetSection(TOPICTITLEGERMAN).Value, topicTitleInGerman ?? string.Empty)
                .Replace(_configuration.GetSection(TASKADDITIONALDATA).Value, taskAdditonalData)
                .Replace(_configuration.GetSection(TASKADDITIONALDATAGERMAN).Value, taskAdditonalDataGerman)
                .Replace(_configuration.GetSection(TASKADDITIONALHEADER).Value, taskAdditionalHeader)
                .Replace(_configuration.GetSection(TASKADDITIONALHEADERGERMAN).Value, taskAdditionalHeaderGerman)
                .Replace(_configuration.GetSection(TASKUPDATEDBY).Value, modifiedBy)
                .Replace(TASKHEADER, GetTaskHeader());

                return returnResult;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        private string TokenReplacement(string source, string to, string person, string link, string title, string role, string committee, string status, bool demouser, DateTime? duedate, long? task, string taskDescription, string closureComment, string rejectionComment, string meetingName, string meetingDate, string meetingLink, long meetingId, bool isConfidential, string emailAddress, string taskResponsible, string taskCoResponsible, string topicTitleInEnglish, string topicTitleInGerman, bool? isSubTask, string modifiedBy = null)
        {
            string tasknotificationLink = link + "?id=" + task;
            string meetingnotificationLink = link + "?id=" + meetingId;
            string demoEmailContent = string.Empty;
            string meetingLinkInfo = string.Empty;
            string meetingLinkInfoGerman = string.Empty;
            if (demouser)
            {
                demoEmailContent = GetDemoContent();
            }

            string returnResult = string.Empty;

            if (meetingId > 0)
            {
                if ((_configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value) != null)
                    meetingnotificationLink = ProccessLink(_configuration.GetSection(EMAILNOTIFICATIONMEETINGACTIONLINK).Value, meetingId.ToString());

                meetingLinkInfo = InsertMeetingLink(meetingId, meetingnotificationLink, false);
                meetingLinkInfoGerman = InsertMeetingLink(meetingId, meetingnotificationLink, true);
            }
            string taskHeader = InsertTaskHeader(meetingId, false);
            string taskHeaderGerman = InsertTaskHeader(meetingId, true);
            string taskData = InsertTaskData(title, status, to, meetingId, meetingName, meetingDate);
            string taskAdditionalHeader = InsertTaskAdditionalHeader(meetingId, false, isSubTask ?? false);
            string taskAdditionalHeaderGerman = InsertTaskAdditionalHeader(meetingId, true, isSubTask ?? false);
            string taskAdditonalData = InsertTaskAdditionalData(taskResponsible ?? string.Empty, taskCoResponsible ?? string.Empty, topicTitleInEnglish ?? string.Empty, meetingId, isSubTask ?? false);
            string taskAdditonalDataGerman = InsertTaskAdditionalDataGerman(taskResponsible ?? string.Empty, taskCoResponsible ?? string.Empty, topicTitleInGerman ?? string.Empty, meetingId, isSubTask ?? false);
            string closureCommentElement = string.Empty;
            string rejectionCommentElement = string.Empty;
            if (!string.IsNullOrEmpty(closureComment))
                closureCommentElement = ClosureCommentElement(closureComment);
            if (!string.IsNullOrEmpty(rejectionComment))
                rejectionCommentElement = RejectionCommentElement(rejectionComment);

            try
            {
                var statusLink = _configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value;
                returnResult = source.Replace(_configuration.GetSection(EMAILNOTIFICATIONTO).Value, to)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONPERSON).Value, person)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASKLINK).Value, tasknotificationLink)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTITLE).Value, title)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONROLE).Value, role)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCOMMITTEE).Value, committee)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONDUEDATE).Value, duedate.HasValue ? duedate.Value.ToString("dd.MM.yyyy") : "")
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONSTATUS).Value, status)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONDEMO).Value, demoEmailContent)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASK).Value, task == (int?)SendEmailTaskType.SUBTASK_TASK_DELETE ? "Sub-Task" : "Task")
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONTASKDESCRIPTION).Value, taskDescription)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONCLOSURECOMMENT).Value, closureCommentElement)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONREJECTIONCOMMENT).Value, rejectionCommentElement)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONMEETINGNAME).Value, meetingName)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONMEETINGDATE).Value, meetingDate)
                .Replace(_configuration.GetSection(EMAILNOTIFICATIONMEETINGLINK).Value, meetingnotificationLink)
                .Replace(_configuration.GetSection(MEETINGHEADER).Value, taskHeader)
                .Replace(_configuration.GetSection(MEETINGDATA).Value, taskData)
                .Replace(_configuration.GetSection(MEETINGLINKINFO).Value, meetingLinkInfo)
                .Replace(_configuration.GetSection(CONFIDENTIAL).Value, isConfidential ? "Confidential" : "")
                .Replace(_configuration.GetSection(CREATOREMAIL).Value, emailAddress)
                .Replace(_configuration.GetSection(DEMOCONTENTGERMAN).Value, demoEmailContent)
                .Replace(_configuration.GetSection(TASKTITLEGERMAN).Value, title)
                .Replace(_configuration.GetSection(TASKDESCRIPTIONGERMAN).Value, taskDescription)
                .Replace(_configuration.GetSection(DUEDATEGERMAN).Value, duedate.HasValue ? duedate.Value.ToString("dd.MM.yyyy") : "")
                .Replace(_configuration.GetSection(CREATORGERMAN).Value, person)
                .Replace(_configuration.GetSection(CREATOREMAILGERMAN).Value, emailAddress)
                .Replace(_configuration.GetSection(TASKACTIONLINKGERMAN).Value, tasknotificationLink)
                .Replace(_configuration.GetSection(MEETINGNAMEGERMAN).Value, meetingName)
                .Replace(_configuration.GetSection(MEETINGDATEGERMAN).Value, meetingDate)
                .Replace(_configuration.GetSection(MEETINGACTIONLINKGERMAN).Value, meetingnotificationLink)
                .Replace(_configuration.GetSection(MEETINGHEADERGERMAN).Value, taskHeaderGerman)
                .Replace(_configuration.GetSection(MEETINGDATAGERMAN).Value, taskData)
                .Replace(_configuration.GetSection(MEETINGLINKINFOGERMAN).Value, meetingLinkInfoGerman)
                .Replace(_configuration.GetSection(CONFIDENTIALGERMAN).Value, isConfidential ? "Vertraulich" : "")
                .Replace(_configuration.GetSection(TASKRESPONSIBLE).Value, taskResponsible ?? to ?? string.Empty)
                .Replace(_configuration.GetSection(TASKCORESPONSIBLE).Value, taskCoResponsible ?? string.Empty)
                .Replace(_configuration.GetSection(TOPICTITLEINENGLISH).Value, topicTitleInEnglish ?? string.Empty)
                .Replace(_configuration.GetSection(TOPICTITLEGERMAN).Value, topicTitleInGerman ?? string.Empty)
                .Replace(_configuration.GetSection(TASKADDITIONALDATA).Value, taskAdditonalData)
                .Replace(_configuration.GetSection(TASKADDITIONALDATAGERMAN).Value, taskAdditonalDataGerman)
                .Replace(_configuration.GetSection(TASKADDITIONALHEADER).Value, taskAdditionalHeader)
                .Replace(_configuration.GetSection(TASKADDITIONALHEADERGERMAN).Value, taskAdditionalHeaderGerman)
                .Replace(_configuration.GetSection(TASKUPDATEDBY).Value, modifiedBy)
                .Replace(TASKHEADER, GetTaskHeader());

                return returnResult;

            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string ConcadenatedMeetingName(string meetingName)
        {
            string proccessMeetingName = !string.IsNullOrEmpty(meetingName) ? meetingName : string.Empty;
            return proccessMeetingName;
        }

        private string ClosureCommentElement(string closureComment)
        {
            StringBuilder closureCommentElement = new StringBuilder();
            closureCommentElement.Append("<td style='padding: 0 2em; padding-bottom: 20px;'>");
            closureCommentElement.Append("<span style='font-weight: bold; font-size: 14px; line-height: 16px; display: block; padding: 0px 0px 0px; text-align: left; color: #000; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; '>");
            closureCommentElement.Append("Closure comment: ");
            closureCommentElement.Append("</span>");
            closureCommentElement.Append("<span style='font-size: 14px; line-height: 16px;display: block;padding: 0px 0px 0px;text-align: left;color: #000;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;'>");
            closureCommentElement.Append(closureComment);
            closureCommentElement.Append("</span><br />");
            closureCommentElement.Append("</td>");
            return closureCommentElement.ToString();
        }

        private string RejectionCommentElement(string rejectionComment)
        {
            StringBuilder rejectionCommentElement = new StringBuilder();
            rejectionCommentElement.Append("<td style='padding: 0 2em; padding-bottom: 20px;'>");
            rejectionCommentElement.Append("<span style='font-weight: bold; font-size: 14px; line-height: 16px; display: block; padding: 0px 0px 0px; text-align: left; color: #000; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; '>");
            rejectionCommentElement.Append("Rejection comment: ");
            rejectionCommentElement.Append("</span>");
            rejectionCommentElement.Append("<span style='font-size: 14px; line-height: 16px;display: block;padding: 0px 0px 0px;text-align: left;color: #000;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;'>");
            rejectionCommentElement.Append(rejectionComment);
            rejectionCommentElement.Append("</span><br />");
            rejectionCommentElement.Append("</td>");
            return rejectionCommentElement.ToString();
        }

        private string ConcadenatedMeetingDate(string meetingDate)
        {

            string proccessMeetingDate = string.Empty;
            proccessMeetingDate = !string.IsNullOrEmpty(meetingDate) ? meetingDate : string.Empty;
            if (!string.IsNullOrEmpty(meetingDate))
            {
                var condatetime = Convert.ToDateTime(meetingDate).ToString("dd-MM-yyyy").Replace("-", ".");
                var listofdate = condatetime.Split(" ");
                if (listofdate != null && listofdate.Length > 0)
                    proccessMeetingDate = listofdate[0];
            }
            return proccessMeetingDate;
        }

        private string ProccessLink(string meetinglink, string meetingid)
        {

            string proccessLink = string.Empty, proccessId;
            proccessLink = !string.IsNullOrEmpty(meetinglink) ? meetinglink : string.Empty;
            proccessId = !string.IsNullOrEmpty(meetingid) ? meetingid : string.Empty;
            if (!string.IsNullOrEmpty(meetinglink))
            {
                var listMeetingLink = meetinglink.Split("id=;");
                if (listMeetingLink != null && listMeetingLink.Length > 0)
                    proccessLink = listMeetingLink[0] + "id=" + proccessId + ";" + listMeetingLink[1];
            }
            return proccessLink;
        }

        private string InsertMeetingLink(long mId, string meetingnotificationLink, bool isGerman)
        {
            string meetinglink = string.Concat("<a href='", meetingnotificationLink, "' style='color: #ffffff; text-decoration:none'>").ToString();
            StringBuilder processMeetingLink = new StringBuilder();
            processMeetingLink.Append("<tr>");
            processMeetingLink.Append("<td style='padding: 0 2.1em 0.4em' align='left'>");
            processMeetingLink.Append("<br />");
            processMeetingLink.Append("<span style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;'>");
            processMeetingLink.Append(isGerman ? "Für mehr Informationen zu Ihrem Meeting." : "For more information on your Meeting");
            processMeetingLink.Append("</span><br />");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            processMeetingLink.Append("<tr style='padding-top:8px'>");
            processMeetingLink.Append("<td bgcolor='#FFFFFF' style='padding: 0 2em;'>");
            processMeetingLink.Append("<table border='0' cellpadding='0' cellspacing='0' width='200px'>");
            processMeetingLink.Append("<tr>");
            processMeetingLink.Append("<td align='center' height='32' style=' padding-right:24px;padding:0px 12px; height:32px; font-size:14px; line-height:12px;width:180px;background-color:#087a94;color:#ffffff;border-radius:18px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;	font-weight:bold;border:none;'>");
            processMeetingLink.Append(meetinglink);
            processMeetingLink.Append(isGerman ? "Gehe zu eLite Meeting" : "Go to eLite Meeting");
            processMeetingLink.Append("</a>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("<td bgcolor='#ffffff'>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("<td bgcolor='#ffffff'>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            processMeetingLink.Append("</table>");
            processMeetingLink.Append("</td>");
            processMeetingLink.Append("</tr>");
            return processMeetingLink.ToString();
        }

        private string InsertTaskHeader(long meetingId, bool isGerman)
        {
            StringBuilder proccessMeetingHeader = new StringBuilder();
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Aufgabentitel" : "Task Title") + "</th>");
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Status" : "Status") + "</th>");
            proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left;'>" + (isGerman ? "Aktualisiert von" : "Updated By") + "</th>");
            if (meetingId > 0)
            {
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Meetingbezeichnung" : "Meeting Name") + "</th>");
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Meeting Datum" : "Meeting Date") + "</th>");
            }
            return proccessMeetingHeader.ToString();
        }

        private string InsertTaskData(string taskTitle, string TaskStatusValue, string responsible, long meetingId, string meetingName,
                    string meetingDate)
        {
            StringBuilder proccessMeetingDate = new StringBuilder();
            if (meetingId > 0)
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='20%'>");
                proccessMeetingDate.Append(taskTitle);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='13%'>");
                proccessMeetingDate.Append(TaskStatusValue);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top;' width='27%'>");
                proccessMeetingDate.Append(responsible);
                proccessMeetingDate.Append("</td>");

                proccessMeetingDate.Append("<td style='font-size:14px;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left; vertical-align: top; padding: 0 0.5em 0 0; ' width='25%'>");
                proccessMeetingDate.Append(meetingName);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size:14px;font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left; vertical-align: top;' width='15%'>");
                proccessMeetingDate.Append(meetingDate);
                proccessMeetingDate.Append("</td>");
            }
            else
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='35%'>");
                proccessMeetingDate.Append(taskTitle);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0; ' width='30%'>");
                proccessMeetingDate.Append(TaskStatusValue);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top;' width='35%'>");
                proccessMeetingDate.Append(responsible);
                proccessMeetingDate.Append("</td>");
            }
            return proccessMeetingDate.ToString();
        }
      
        private string GetDemoContent()
        {
            StringBuilder demoContent = new StringBuilder();

            demoContent.Append("<table role = 'presentation' border = '0' cellpadding = '0' cellspacing = '0' class='btn btn-default' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; min-width: 100%; box-sizing: border-box; width: 100%;' width='100%'>");
            demoContent.Append("<tbody><tr>");
            demoContent.Append("<td align = 'center' style='font-family: 'Daimler CS', sans-serif; font-size: 16px; vertical-align: top; padding-bottom: 15px;'valign ='top'>");
            demoContent.Append("<table role = 'presentation' border='0' cellpadding='0' cellspacing='0' style='border-collapse: separate; mso-table-lspace: 0pt; mso-table-rspace: 0pt; min-width: auto; width: auto;'>");
            demoContent.Append("<tbody><tr>");
            demoContent.Append("<td style =  'font-family: 'MB Corpo S Text Office', sans-serif; font-size: 20px; vertical-align: top; border-radius: 0px; text-align: center; background-color: transparent; padding: 15px;' valign='top' align='center' bgcolor='#e6e6e6'>");
            demoContent.Append("<a href = '#'  style='border-radius: 0px; text-decoration:underline; box-sizing: border-box; cursor: pointer; display: inline-block; font-size: 20px; font-weight: normal; margin: 0; padding: 6px 25px;  text-transform: capitalize; background-color: transparent; border: 2px solid #696969; color: #696969; font-weight:bold'>");
            demoContent.Append("Trial Version");
            demoContent.Append("</a></ td ></tr>");
            demoContent.Append("</tbody></ table ></td>");
            demoContent.Append("</tr></tbody></table>");
            return demoContent.ToString();
        }

        private string GetTaskHeader()
        {
            StringBuilder headerContent = new StringBuilder();

            if (string.IsNullOrWhiteSpace(poolIdEmailId))
                headerContent.Append("<td style='background: #000; padding: 0px 5em; padding-bottom: 3px; text-align:center; width:100%'>")
                    .Append("<span style='color: #c0c0c0; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; font-size: 11px;'>")
                    .Append("***This is an automatically generated e-mail by eLite application, please do not reply to this e-mail*** </span></td>");
            else
                headerContent.Append("<tr><td style='background: #000; padding: 0px 5em; padding-bottom: 3px; text-align:center; width:100%; height: 20px'></td></tr>");

            return headerContent.ToString();
        }
        private void AddRecipient(SendEmail sendEmailObj, string email)
        {
            if (!sendEmailObj.Receipients.Contains(email))
                sendEmailObj.Receipients.Add(email);
        }
        //taskAdditionalData for task complete template
        private string InsertTaskAdditionalHeader(long meetingId, bool isGerman, bool isSubtask)
        {
            StringBuilder proccessMeetingHeader = new StringBuilder();
            if (meetingId > 0)
            {
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Thema titel" : "Topic Title") + "</th>");
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Verantwortlich" : "Responsible") + "</th>");
                if (!isSubtask)
                {
                    proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Mitverantwortlich" : "Co-Responsible") + "</th>");
                }
            }
            else
            {
                proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Verantwortlich" : "Responsible") + "</th>");
                if (!isSubtask)
                {
                    proccessMeetingHeader.Append("<th style='font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif;text-align:left;'>" + (isGerman ? "Mitverantwortlich" : "Co-Responsible") + "</th>");
                }
            }
            return proccessMeetingHeader.ToString();
        }

        private string InsertTaskAdditionalDataCommon(string taskResponsible, string taskCoResponsible, string topicTitle, long meetingId, bool isSubtask)
        {
            StringBuilder proccessMeetingDate = new StringBuilder();

            if (meetingId > 0)
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0;' width='35%'>");
                proccessMeetingDate.Append(topicTitle);
                proccessMeetingDate.Append("</td>");
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0;' width='35%'>");
                proccessMeetingDate.Append(taskResponsible);
                proccessMeetingDate.Append("</td>");
                if (!isSubtask)
                {
                    proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.1em 0 0;' width='30%'>");
                    proccessMeetingDate.Append(taskCoResponsible);
                    proccessMeetingDate.Append("</td>");
                }
            }
            else
            {
                proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.5em 0 0;' width='50%'>");
                proccessMeetingDate.Append(taskResponsible);
                proccessMeetingDate.Append("</td>");
                if (!isSubtask)
                {
                    proccessMeetingDate.Append("<td style='font-size: 14px; font-family: MB Corpo S Text Office, Arial, Helvetica, sans-serif; text-align: left; vertical-align: top; padding: 0 0.1em 0 0;' width='55%'>");
                    proccessMeetingDate.Append(taskCoResponsible);
                    proccessMeetingDate.Append("</td>");
                }
            }

            return proccessMeetingDate.ToString();
        }

        private string InsertTaskAdditionalData(string taskResponsible, string taskCoResponsible, string topicTitle, long meetingId, bool isSubtask)
        {
            return InsertTaskAdditionalDataCommon(taskResponsible, taskCoResponsible, topicTitle, meetingId, isSubtask);
        }

        private string InsertTaskAdditionalDataGerman(string taskResponsible, string taskCoResponsible, string topicTitle, long meetingId, bool isSubtask)
        {
            return InsertTaskAdditionalDataCommon(taskResponsible, taskCoResponsible, topicTitle, meetingId, isSubtask);
        }

    }
}
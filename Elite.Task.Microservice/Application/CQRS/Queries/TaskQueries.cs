
using DinkToPdf.Contracts;
using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.DocumentCloud;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.HtmlAgilityPack;
using Elite.Common.Utilities.ListDownload;
using Elite.Common.Utilities.Paging;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.SearchFilter;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.Filters.Lib.Services;
using Elite.ListToPDF;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Application.CQRS.Queries;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.Application.SearchFilter;
using Elite.Task.Microservice.CommonLib;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.Application.Paging;
using Elite_Task.Microservice.Application.SearchFilter;
using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MoreLinq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;
using TaskDueStatus = Elite_Task.Microservice.CommonLib.TaskDueStatus;
using Elite.Task.Microservice.Models.Entities;
using Npgsql;

namespace Elite_Task.Microservice.Application.CQRS.Queries
{
    public class TaskQueries : BaseDataAccess, ITaskQueries
    {
        #region Variable declarations

        private readonly IConfiguration _configuration;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;
        private readonly IRequestContext _requestContext;
        //private IList<UserRolesAndRights> userRolesAndRights;
        private IList<CommitteeDetail> committees;

        //private bool _isAdmin = false;
        //private bool _isTransient = false;
        private string _UID;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;

        private readonly Func<DbConnection, IFilterQueries> _filtersServiceFactory;
        private readonly IFilterQueries _filtersService;
        //#endregion
        //private bool _isAdmin = false;
        //private bool _isTransient = false;       
        IConverter _converter;
        private bool isEliteClassic;
        private readonly ITaskCommentQueries _taskQueries;

        #endregion

        #region Constructors
        public TaskQueries(EliteTaskContext context, IConfiguration configuration, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory, IRequestContext requestContext, Func<DbConnection, IFilterQueries> filtersServiceFactory, IConverter converter, ITaskCommentQueries taskQueries) : base(context)
        {
            _configuration = configuration;
            _userServiceFactory = userServiceFactory;
            _userService = userServiceFactory(configuration, requestContext);
            _requestContext = requestContext;
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            _filtersServiceFactory = filtersServiceFactory;
            _filtersService = this._filtersServiceFactory(context.Database.GetDbConnection());
            _converter = converter;
            InitializeUsers();
            isEliteClassic = _configuration.GetSection("isEliteClassic").Get<bool>();
            _taskQueries = taskQueries;
        }
        #endregion           

        #region Public Functions

        /// <summary>
        /// Get Topics By Pagination
        /// </summary>
        /// <param name="page"> current page index </param>
        /// <param name="listType"> Card Type Or List type</param>
        /// <param name="searchKeywords"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<IList<QueriesTaskListDto>> GetTaskByPagination(int? page, int listType, TaskSearchKeywords searchKeywords, HttpResponse response, FilterActionEnum topicFilterAction)
        {
            try
            {
                ITaskRolesAndRightFilterBuilder<EliteTask> rolesBuilder = TaskRolesAndRightsFilterFactory.GetTaskRolesAndRightsFilterObject(topicFilterAction, this.rolesPermissions.UserRolesAndRights, this._requestContext, isEliteClassic);
                if (rolesBuilder is null)
                    throw new EliteException($"{nameof(ITaskRolesAndRightFilterBuilder<EliteTask>)} should not be empty");

                int pageSize = PageHelper.GetTopicListPageSize(listType, _configuration);
                var tasks = PaginatedList<EliteTask>.Create(await GetTaskByPaginate(this.rolesPermissions.IsAdmin ? null : rolesBuilder.BuildFilter(), searchKeywords), page ?? 1, pageSize);
                response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(getPageMetaData(tasks)));

                if (tasks?.Count > 0)
                {
                    return GetTaskListDtos(tasks);
                }
                return null;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public async Task<IList<QueriesTaskListDto>> GetTasks(TaskSearchKeywords searchKeywords)
        {
            var rolesBuilder = new TaskUserRolesAndRightFilterBuilder<EliteTask>(this.rolesPermissions.UserRolesAndRights, _requestContext, isEliteClassic);
            var builder = new TaskFilterCollectionBuilder<EliteTask>(searchKeywords);
            var tasks = await GetTaskByPaginateForList(builder.BuildFilter(), this.rolesPermissions.IsAdmin ? null : rolesBuilder.BuildFilter());

            // var tasks = PaginatedList<EliteTask>.Create(GetTaskByPaginateForList(builder.BuildFilter(), this._isAdmin ? null : rolesBuilder.BuildFilter()), 1, 20).ToList();

            if (tasks?.Count > 0)
            {
                if (this.rolesPermissions.IsAdmin)
                {

                    return (from p in tasks
                            let subTaskCount = (long)p.InverseParent?.Count
                            select new QueriesTaskListDto
                            {
                                Id = p.Id,
                                Title = p.Title,
                                DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                                TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                                HasSubTask = subTaskCount > 0 ? true : false,
                                SubTaskCount = subTaskCount,
                                HasMeetingTask = p.MeetingId.HasValue,
                                MeetingId = p.MeetingId,
                                MeetingStatus = p.MeetingStatus,
                                IsPublishedToJira = p.IsPublishedToJira,
                                JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null
                            }).ToList();
                }
                else
                {
                    return (from p in tasks
                            join ro in this.rolesPermissions.UserRolesAndRights on p.CommitteeId equals ro.CommitteeId into joinedV
                            let subTaskCount = (long)p.InverseParent?.Count
                            from v in joinedV.DefaultIfEmpty()
                            join y in this.committees on p.CommitteeId equals y.CommitteeId into joinedY
                            from y in joinedY.DefaultIfEmpty()
                            select new QueriesTaskListDto
                            {
                                Id = p.Id,
                                Title = p.Title,
                                DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                                TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                                HasSubTask = subTaskCount > 0 ? true : false,
                                SubTaskCount = subTaskCount,
                                HasMeetingTask = p.MeetingId.HasValue,
                                MeetingId = p.MeetingId,
                                MeetingStatus = p.MeetingStatus,
                                CommitteeName = (y != null) ? y.CommitteeName : string.Empty
                            }).ToList();
                }
            }
            return null;
        }


        public async Task<QueriesTaskDto> GetTaskAsync(long id)
        {
            return await GetTaskDto(id);
        }

        public async Task<dynamic> GetTaskDescription(long taskID)
        {
            if (taskID != 0)
            {
                return await this.GetDBSet<EliteTask>().Where(x => x.Id.Equals(taskID)).Select(s => s.Description).ToListAsync();
            }
            else
            {
                return string.Empty;
            }
        }


        public async Task<IList<QueriesDeshboardTask>> GetTaskForDashboard()
        {
            var rolesBuilder = new TaskUserRolesAndRightFilterBuilder<EliteTask>(this.rolesPermissions.UserRolesAndRights, _requestContext, isEliteClassic, true);
            var tasks = await GetTaskByPaginateForDashBoard(this.rolesPermissions.IsAdmin || this.rolesPermissions.IsTransient ? null : rolesBuilder.BuildFilter());
            if (tasks?.Count > 0)
                return (tasks
                               .Select(p => new QueriesDeshboardTask
                               {
                                   Id = p.Id,
                                   ParentId = p.ParentId,
                                   Title = p.Title,
                                   Description = HtmlAgilityUtility.ConvertHTMLToString(p.Description) ?? string.Empty,
                                   DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                   Responsible = JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible),
                                   CoResponsible = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                   Status = p.Status,
                                   CreatedBy = JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy)
                               })).ToList();
            return null;
        }


        public async Task<IList<QueriesPDFTaskDto>> GetPDFTask(long[] ids)
        {
            return await (from p in this.GetDBSet<EliteTask>()
        .Include(EliteTask.PATH_EliteTaskAttachment)
        .Include(EliteTask.PATH_EliteSubTask)
                          where ids.Contains(p.Id)
                          select new QueriesPDFTaskDto
                          {
                              Id = p.Id,
                              Title = p.Title,
                              Description = p.Description ?? string.Empty,
                              DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                              Responsible = JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible),
                              CoResponsible = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                              CreatedBy = JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy),
                              SubTask = p.InverseParent.Select(x => GetSubTaskPDF(x)),
                          }).AsQueryable()
                       .AsNoTracking()
                       .ToListAsync();

        }

        public async Task<IList<LookUp>> GetAllUserCommitteesForTask(string uid)
        {
            var data = await _context.user_committees_in_tasks.FromSql("SELECT * FROM elite.user_committees_in_tasks({0})", uid).ToListAsync();

            var topicCommittesList = (from p in data
                                      join cm in this.committees on p.CommitteeId equals cm.CommitteeId
                                      select new
                                      {
                                          Id = cm.CommitteeId,
                                          Value = cm.CommitteeName
                                      }).GroupBy(g => new { g.Id, g.Value })
                          .Where(x => x.Key.Id != -1)
                          .Select(s => new LookUp
                          {
                              Id = s.Key.Id,
                              Value = s.Key.Value
                          }).ToList();



            List<LookUp> userCommittesList = await _userService.GetUserCommittees();
            if (userCommittesList.Count > 0)
            {
                topicCommittesList = topicCommittesList.Concat(userCommittesList).DistinctBy(x => x.Id).ToList();
            }
            return topicCommittesList;
        }

        public async Task<dynamic> GetFilters()
        {
            return await _filtersService.GetAsync(this._UID, FilterType.Task);
        }

        #endregion

        #region Private Functions

        private List<EntityAction> GetUserAction(bool isCMComUser, long? meetingId, string createdby, int? roleId)
        {
            if (isEliteClassic)
            {
                if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager))
                    return _configuration.GetSection("ContextMenuFullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && roleId.Value == (int)RolesType.User)
                {
                    if (createdby.ToUpper().Contains(this._UID) && !meetingId.HasValue)
                        return _configuration.GetSection("ContextMenuFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("ContextMenuPartialPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && roleId.Value == (int)RolesType.CoreMember)
                {
                    if (createdby.ToUpper().Contains(this._UID) && !meetingId.HasValue)
                        return _configuration.GetSection("ContextMenuFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("ContextMenuPartialPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (isCMComUser && createdby.ToUpper().Contains(this._UID) && !meetingId.HasValue)
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                else if (isCMComUser)
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
            }
            else
            {
                if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager))
                    return _configuration.GetSection("ContextMenuFullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.User ||
                            roleId.Value == (int)RolesType.CoreMemberExternal || roleId.Value == (int)RolesType.Guest))
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember ||
                        roleId.Value == (int)RolesType.AssistantDocumentMember))
                {
                    if (!meetingId.HasValue)
                        return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (isCMComUser && createdby.ToUpper().Contains(this._UID) && !meetingId.HasValue)
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                else if (isCMComUser)
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>();
            }
        }

        private IList<EntityAction> GetUserAction(string createdby, string responsible, string coresponsible, bool hasSubTask, int? roleId, bool hasMeeting)
        {
            if (isEliteClassic)
            {
                if (roleId.HasValue && roleId.Value == (int)RolesType.Transient)
                {
                    if (responsible.Contains(this._UID) || (coresponsible != null && coresponsible.Contains(this._UID)))
                        return _configuration.GetSection("TaskResponsibleTransientPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.Admin))
                    return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember))
                {
                    if (roleId.HasValue && createdby.Contains(this._UID))
                        return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                    else if (responsible.Contains(this._UID) || (coresponsible != null && coresponsible.Contains(this._UID)))
                        return _configuration.GetSection("CoreMemberPartialPermissions:Actions").Get<List<EntityAction>>();
                    else if (!hasMeeting)
                        return _configuration.GetSection("PartialFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("MeetingPartialFullPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.User))
                {
                    if (roleId.HasValue && createdby.Contains(this._UID))
                        return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                    else if (responsible.Contains(this._UID) || (coresponsible != null && coresponsible.Contains(this._UID)))
                        return _configuration.GetSection("CoreMemberPartialPermissions:Actions").Get<List<EntityAction>>();
                    else if (!hasMeeting)
                        return _configuration.GetSection("PartialFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("MeetingPartialFullPermissions:Actions").Get<List<EntityAction>>();
                }
                if (createdby.Contains(this._UID))
                    return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                else if (coresponsible != null ? coresponsible.Contains(this._UID) : false)
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (responsible.Contains(this._UID))
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (hasSubTask)
                    return _configuration.GetSection("PartialPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("LessPermissions:Actions").Get<List<EntityAction>>();
            }
            else
            {
                if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMemberExternal ||
                    roleId.Value == (int)RolesType.User || roleId.Value == (int)RolesType.Guest))
                {
                    if (((responsible != null && responsible.Contains(this._UID)) || (coresponsible != null && coresponsible.Contains(this._UID))) &&
                       (roleId.Value == (int)RolesType.User || roleId.Value == (int)RolesType.Guest))
                    {
                        return _configuration.GetSection("LessPermissions:Actions").Get<List<EntityAction>>();
                    }
                    else
                    {
                        return _configuration.GetSection("NoPermissions:Actions").Get<List<EntityAction>>();
                    }
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.AssistantDocumentMember))
                {
                    if (!hasMeeting)
                        return _configuration.GetSection("ADMPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("ADMMeetingPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.Admin))
                    return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember))
                {
                    if (!hasMeeting)
                        return _configuration.GetSection("PartialFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("MeetingPartialFullPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (createdby.Contains(this._UID))
                    return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && roleId.Value == (int)RolesType.Transient && responsible.Contains(this._UID))
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && roleId.Value == (int)RolesType.Transient && coresponsible != null ? coresponsible.Contains(this._UID) : false)
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (coresponsible != null ? coresponsible.Contains(this._UID) : false)
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (responsible.Contains(this._UID))
                    return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
                else if (hasSubTask)
                    return _configuration.GetSection("PartialPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("NoPermissions:Actions").Get<List<EntityAction>>();
            }
        }

        private IList<EntityAction> GetUserSubTaskAction(string createdby, string responsible, int? roleId, string subTaskResponsible, string subTaskCreatedby, bool isMeetingTask)
        {
            if (isEliteClassic)
            {
                if (roleId.HasValue && roleId.Value == (int)RolesType.Transient)
                {
                    if (subTaskResponsible.Contains(this._UID) && subTaskCreatedby.Contains(this._UID))
                        return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                    else if (subTaskResponsible.Contains(this._UID))
                        return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.Admin))
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember))
                    if (createdby.Contains(this._UID) || subTaskCreatedby.Contains(this._UID) || subTaskResponsible.Contains(this._UID))
                    {
                        return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                    }
                    else
                    {
                        return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
                    }
                else if (roleId.HasValue && roleId.Value == (int)RolesType.User)
                {
                    if (createdby.Contains(this._UID) || subTaskCreatedby.Contains(this._UID) || subTaskResponsible.Contains(this._UID))
                        return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                    {
                        return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
                    }
                }
                if (createdby.Contains(this._UID) || subTaskCreatedby.Contains(this._UID) || subTaskResponsible.Contains(this._UID))
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else if (subTaskResponsible.Contains(this._UID))
                    return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
            }
            else
            {
                if (roleId.HasValue && roleId.Value == (int)RolesType.Transient)
                {
                    if (responsible.Contains(this._UID) && subTaskCreatedby.Contains(this._UID))
                        return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                    else if (subTaskResponsible.Contains(this._UID) || responsible.Contains(this._UID))
                        return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.Admin))
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember))
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMemberExternal || roleId.Value == (int)RolesType.Guest || roleId.Value == (int)RolesType.User))
                    return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
                else if (roleId.HasValue && (roleId.Value == (int)RolesType.AssistantDocumentMember))
                {
                    if (!isMeetingTask)
                        return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                    else
                        return _configuration.GetSection("SubTaskADMMeetingPermissions:Actions").Get<List<EntityAction>>();
                }
                else if ((createdby.Contains(this._UID)) || (subTaskCreatedby.Contains(this._UID) && responsible.Contains(this._UID)))
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else if (subTaskResponsible.Contains(this._UID) || responsible.Contains(this._UID))
                    return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
            }
        }

        private int? GetRole()
        {
            if (this.rolesPermissions.IsAdmin) return (int?)RolesType.Admin;
            else if (this.rolesPermissions.IsTransient) return (int?)RolesType.Transient;
            return null;
        }

        private void InitializeUsers()
        {
            Task.Run(async () =>
            {
                this._UID = this._requestContext.IsDeputy ? _requestContext.DeputyUID.Upper() : _requestContext.UID.Upper();
                this.committees = await _userService.GetCommitees();
            }).Wait();
        }

        private QueriesTaskAttachmentDto GetAttachment(TaskAttachmentMapping topicsAttachment)
        {
            return new QueriesTaskAttachmentDto()
            {
                Id = topicsAttachment.Id,
                AttachmentDesc = topicsAttachment.AttachmentName,
                AttachmentSize = (topicsAttachment.AttachmentSize.HasValue) ? topicsAttachment.AttachmentSize.Value : (long?)null,
                AttachmentGuid = topicsAttachment.AttachmentGuid,
                IsDeleted = false
            };
        }

        private QueriesSubTaskDto GetSubTask(string createdby, string responsible, int? roleId, EliteTask subTask, int commentCount, bool isMeetingTask)
        {
            return new QueriesSubTaskDto()
            {
                Id = subTask.Id,
                Title = subTask.Title,
                Description = subTask.Description,
                DueDate = subTask.DueDate.HasValue ? subTask.DueDate.Value : (DateTime?)null,
                Responsible = JsonConvert.DeserializeObject<QueriesPersonDto>(subTask.Responsible),
                Status = subTask.Status,
                ParentId = subTask.ParentId,
                CreatedBy = !string.IsNullOrWhiteSpace(Convert.ToString(subTask.CreatedBy)) ? JsonConvert.DeserializeObject<QueriesPersonDto>(subTask.CreatedBy) : null,
                CreateDate = subTask.CreatedDate,
                ModifiedDate = subTask.ModifiedDate,
                ModifiedBy = !string.IsNullOrWhiteSpace(Convert.ToString(subTask.ModifiedBy)) ? JsonConvert.DeserializeObject<QueriesPersonDto>(subTask.ModifiedBy) : null,
                CommentCount = commentCount,
                Actions = GetUserSubTaskAction(createdby.Upper(), responsible.Upper(), roleId, subTask.Responsible.Upper(), subTask.CreatedBy.Upper(), isMeetingTask)
            };
        }

        private QueriesSubTaskPDFTaskDto GetSubTaskPDF(EliteTask subTask)
        {
            return new QueriesSubTaskPDFTaskDto()
            {
                Id = subTask.Id,
                Title = subTask.Title,
                Description = subTask.Description,
                DueDate = subTask.DueDate.HasValue ? subTask.DueDate.Value : (DateTime?)null,
                Responsible = JsonConvert.DeserializeObject<QueriesPersonDto>(subTask.Responsible)
            };
        }

        private async Task<IQueryable<EliteTask>> GetTaskByPaginate(Expression<Func<EliteTask, bool>> roles = null, TaskSearchKeywords keys = null)
        {
            try
            {
                IQueryable<EliteTask> tasklistWithSubtask = _context.user_tasks.FromSql("SELECT * FROM elite.user_tasks({0},{1},{2})", _UID, keys.TaskTitle, keys.TaskFilterType == TaskFilterType.EliteTask ? false : true).AsNoTracking();


                //Roles - User, Core Member and Committee Manager 
                if (roles != null)
                {
                    //var tasklist = tasklistWithSubtask.Where(x => x.ParentId == null);//get only task (exclude subtask) to show in list
                    var returnvalue = tasklistWithSubtask
                        .Where(roles)
                        .Select(s => new EliteTask
                        {
                            Id = s.Parent != null ? s.Parent.Id : s.Id,
                            Title = s.Parent != null ? s.Parent.Title : s.Title,
                            ClosureComment = s.Parent != null ? s.Parent.ClosureComment : s.ClosureComment,
                            DueDate = s.Parent != null ? s.Parent.DueDate : s.DueDate,
                            Responsible = s.Parent != null ? s.Parent.Responsible : s.Responsible,
                            ResponsibleEmailRecipient = s.Parent != null ? s.Parent.ResponsibleEmailRecipient : s.ResponsibleEmailRecipient,
                            CoResponsibles = s.Parent != null ? s.Parent.CoResponsibles : s.CoResponsibles != null ? s.CoResponsibles : null,
                            CoResponsibleEmailRecipient = s.Parent != null ? s.Parent.CoResponsibleEmailRecipient : s.CoResponsibleEmailRecipient != null ? s.CoResponsibleEmailRecipient : null,
                            Status = s.Parent != null ? s.Parent.Status : s.Status,
                            MeetingId = s.Parent != null ? s.Parent.MeetingId : s.MeetingId,
                            MeetingDate = s.Parent != null ? s.Parent.MeetingDate : s.MeetingDate,
                            CommitteeId = s.Parent != null ? s.Parent.CommitteeId : s.CommitteeId,
                            CreatedBy = s.Parent != null ? s.Parent.CreatedBy : s.CreatedBy,
                            MeetingStatus = s.Parent != null ? s.Parent.MeetingStatus : s.MeetingStatus,
                            AgendaId = s.Parent != null ? s.Parent.AgendaId : s.AgendaId,
                            FileLink = s.FileLink,
                            IsPublishedToJira = s.Parent != null ? s.Parent.IsPublishedToJira : s.IsPublishedToJira,
                            IsCustomEmailRecipient = s.Parent != null ? s.Parent.IsCustomEmailRecipient : s.IsCustomEmailRecipient,
                            JiraTicketInfo = s.Parent != null ? s.Parent.JiraTicketInfo : s.JiraTicketInfo,
                            ResponsibleDivision = s.Parent != null ? s.Parent.ResponsibleDivision : s.ResponsibleDivision
                        })
                        .ToList()      
                        .DistinctBy(task => task.Id) 
                        .AsQueryable();

                    return returnvalue;
                }
                else
                    //Roles - Admin
                    return this.GetDBSet<EliteTask>()
                                              .Where(x => x.ParentId == null)
                                              .Where(s => s.IsActive != false && s.Action != (int)DatabaseAction.Delete && CheckTaskType(keys, s))
                                              .Select(a => new EliteTask
                                              {
                                                  Id = a.Id,
                                                  Title = a.Title,
                                                  DueDate = a.DueDate,
                                                  Responsible = a.Responsible,
                                                  CoResponsibles = a.CoResponsibles,
                                                  Status = a.Status,
                                                  MeetingId = a.MeetingId,
                                                  MeetingDate = a.MeetingDate,
                                                  SubTaskCount = a.InverseParent.Count,
                                                  CommitteeId = a.CommitteeId,
                                                  CreatedBy = a.CreatedBy,
                                                  MeetingStatus = a.MeetingStatus,
                                                  AgendaId = a.AgendaId,
                                                  IsPublishedToJira = a.IsPublishedToJira,
                                                  JiraTicketInfo = a.JiraTicketInfo,
                                                  ResponsibleDivision = a.ResponsibleDivision
                                              })
                                              .AsNoTracking();
            }
            catch (Exception e)
            {
                return null;
            }


        }


        private async Task<IQueryable<EliteTask>> GetTaskByPaginate(Expression<Func<EliteTask, bool>> predecate, Expression<Func<EliteTask, bool>> roles = null, TaskSearchKeywords keys = null)
        {
            try
            {
                // IQueryable<EliteTask> tasklist = _context.user_tasks.FromSql("SELECT * FROM elite.user_tasks({0},{1})", _UID, "FULL").AsNoTracking();


                //Roles - User, Core Member and Committee Manager
                if (roles != null)
                {
                    var returnvalue = this.GetDBSet<EliteTask>()
                                              .Include(p => p.Parent)
                                              .Where(s => s.IsActive != false && s.Action != (int)DatabaseAction.Delete && CheckTaskType(keys, s))
                                              .Where(roles)
                                              .Select(s => new EliteTask
                                              {
                                                  Id = s.Parent != null ? s.Parent.Id : s.Id,
                                                  Title = s.Parent != null ? s.Parent.Title : s.Title,
                                                  Description = s.Parent != null ? s.Parent.Description : s.Description,
                                                  DueDate = s.Parent != null ? s.Parent.DueDate : s.DueDate,
                                                  Responsible = s.Parent != null ? s.Parent.Responsible : s.Responsible,
                                                  ResponsibleEmailRecipient = s.Parent != null ? s.Parent.ResponsibleEmailRecipient : s.ResponsibleEmailRecipient != null ? s.ResponsibleEmailRecipient : "",
                                                  CoResponsibles = s.Parent != null ? s.Parent.CoResponsibles : s.CoResponsibles != null ? s.CoResponsibles : null,
                                                  CoResponsibleEmailRecipient = s.Parent != null ? s.Parent.CoResponsibleEmailRecipient : s.CoResponsibleEmailRecipient != null ? s.CoResponsibleEmailRecipient : "",
                                                  Status = s.Parent != null ? s.Parent.Status : s.Status,
                                                  MeetingId = s.Parent != null ? s.Parent.MeetingId : s.MeetingId,
                                                  CommitteeId = s.Parent != null ? s.Parent.CommitteeId : s.CommitteeId,
                                                  CreatedBy = s.Parent != null ? s.Parent.CreatedBy : s.CreatedBy,
                                                  MeetingStatus = s.Parent != null ? s.Parent.MeetingStatus : s.MeetingStatus,
                                                  AgendaId = s.Parent != null ? s.Parent.AgendaId : s.AgendaId,
                                                  FileLink = s.FileLink,
                                                  IsPublishedToJira = s.Parent != null ? s.Parent.IsPublishedToJira : s.IsPublishedToJira,
                                                  IsCustomEmailRecipient = s.Parent != null ? s.Parent.IsCustomEmailRecipient : s.IsCustomEmailRecipient,
                                                  JiraTicketInfo = s.Parent != null ? s.Parent.JiraTicketInfo : s.JiraTicketInfo,
                                                  ClosureComment = s.Parent != null ? s.Parent.ClosureComment : s.ClosureComment,
                                                  ResponsibleDivision = s.Parent != null ? s.Parent.ResponsibleDivision : s.ResponsibleDivision,
                                                  MeetingDate = s.Parent !=null ? s.MeetingDate : s.MeetingDate
                                              })
                                              .GroupBy(g => new { g.Id, g.Title, g.Description, g.DueDate, g.Responsible, g.CoResponsibles, g.Status, g.MeetingId, g.CommitteeId, g.CreatedBy, g.MeetingStatus, g.AgendaId, g.FileLink, g.IsPublishedToJira, g.JiraTicketInfo, g.ClosureComment, g.ResponsibleEmailRecipient, g.CoResponsibleEmailRecipient, g.IsCustomEmailRecipient,
                                                  g.ResponsibleDivision,
                                                  g.MeetingDate})
                                              .Select(y => new EliteTask
                                              {
                                                  Id = y.Key.Id,
                                                  Title = y.Key.Title,
                                                  Description = y.Key.Description,
                                                  DueDate = y.Key.DueDate,
                                                  Responsible = y.Key.Responsible,
                                                  ResponsibleEmailRecipient = y.Key.ResponsibleEmailRecipient,
                                                  CoResponsibles = y.Key.CoResponsibles,
                                                  CoResponsibleEmailRecipient = y.Key.CoResponsibleEmailRecipient,
                                                  IsCustomEmailRecipient = y.Key.IsCustomEmailRecipient,
                                                  Status = y.Key.Status,
                                                  MeetingId = y.Key.MeetingId,
                                                  CommitteeId = y.Key.CommitteeId,
                                                  CreatedBy = y.Key.CreatedBy,
                                                  MeetingStatus = y.Key.MeetingStatus,
                                                  SubTaskCount = y.Count(),
                                                  AgendaId = y.Key.AgendaId,
                                                  FileLink = y.Key.FileLink,
                                                  IsPublishedToJira = y.Key.IsPublishedToJira,
                                                  JiraTicketInfo = y.Key.JiraTicketInfo,
                                                  ClosureComment = y.Key.ClosureComment,
                                                  ResponsibleDivision = y.Key.ResponsibleDivision,
                                                  MeetingDate = y.Key.MeetingDate
                                              })
                                              .Where(predecate)
                                              .AsNoTracking();
                    return returnvalue;
                }
                else
                    //Roles - Admin
                    return this.GetDBSet<EliteTask>()
                                              .Where(x => x.ParentId == null)
                                              .Where(s => s.IsActive != false && s.Action != (int)DatabaseAction.Delete && CheckTaskType(keys, s))
                                              .Select(a => new EliteTask
                                              {
                                                  Id = a.Id,
                                                  Title = a.Title,
                                                  Description = a.Description,
                                                  DueDate = a.DueDate,
                                                  Responsible = a.Responsible,
                                                  CoResponsibles = a.CoResponsibles,
                                                  Status = a.Status,
                                                  MeetingId = a.MeetingId,
                                                  SubTaskCount = a.InverseParent.Count,
                                                  CommitteeId = a.CommitteeId,
                                                  CreatedBy = a.CreatedBy,
                                                  MeetingStatus = a.MeetingStatus,
                                                  AgendaId = a.AgendaId,
                                                  IsPublishedToJira = a.IsPublishedToJira,
                                                  JiraTicketInfo = a.JiraTicketInfo,
                                                  ClosureComment = a.ClosureComment,
                                                  ResponsibleDivision = a.ResponsibleDivision,
                                              })
                                              .AsNoTracking();
            }
            catch (Exception e)
            {
                return null;
            }


        }

        private static bool CheckTaskType(TaskSearchKeywords keys, EliteTask t)
        {
            return keys.TaskFilterType == TaskFilterType.JiraTask ? t.IsPublishedToJira == true : t.IsPublishedToJira == false;
        }

        private async Task<IList<EliteTask>> GetTaskByPaginateForList(Expression<Func<EliteTask, bool>> predecate, Expression<Func<EliteTask, bool>> roles = null)
        {
            //Roles - User, Core Member and Committee Manager

            if (roles != null)
            {
                return await this.GetDBSet<EliteTask>()
                    .Include(p => p.Parent)
                    .Where(roles.And(predecate))
                    .Select(s => s.Parent ?? s)
                    .Distinct().ToListAsync();
            }
            //Roles - Admin
            else
            {
                return await (from p in this.GetDBSet<EliteTask>()
                      .Where(x => x.ParentId == null)
                               .Where(predecate)
                              select new EliteTask
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  DueDate = p.DueDate,
                                  Responsible = p.Responsible,
                                  CoResponsibles = p.CoResponsibles,
                                  Status = p.Status,
                                  InverseParent = p.InverseParent,
                                  CommitteeId = p.CommitteeId,
                                  IsPublishedToJira = p.IsPublishedToJira,
                                  JiraTicketInfo = p.JiraTicketInfo
                              }).ToListAsync();
            }
        }

        private async Task<IList<EliteTask>> GetTaskByPaginateForDashBoard(Expression<Func<EliteTask, bool>> roles = null)
        {
            //Roles - User, Core Member and Committee Manager
            if (roles != null)
            {
                try
                {
                    var returnvalue = await OrderBy.ApplyOrderBy(this.GetDBSet<EliteTask>()
                .Where(p => p.Status.Equals((int)CommonLib.TaskStatus.Assigned) || p.Status.Equals((int)CommonLib.TaskStatus.InProgress))
                .Where(s => s.IsActive != false && s.Action != (int)DatabaseAction.Delete)
                .Where(roles)
                .Select(s => new EliteTask
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    DueDate = s.DueDate,
                    Responsible = s.Responsible,
                    CoResponsibles = s.CoResponsibles,
                    Status = s.Status,
                    CreatedBy = s.CreatedBy,
                    ParentId = s.ParentId,
                    IsPublishedToJira = s.IsPublishedToJira,
                    JiraTicketInfo = s.JiraTicketInfo
                })
                , new OrderByExpression<EliteTask, DateTime?>(p => p.DueDate.Value, false),
                      new OrderByExpression<EliteTask, string>(p => p.Title, false))
                .Take(11)
                .AsNoTracking()
                .ToListAsync();
                    return returnvalue;
                }
                catch (Exception e)
                {
                    return null;
                }

            }
            //Roles - Admin, Transient
            else
                return await OrderBy.ApplyOrderBy(this.GetDBSet<EliteTask>()
                .Where(p => p.Status.Equals((int)CommonLib.TaskStatus.Assigned) || p.Status.Equals((int)CommonLib.TaskStatus.InProgress))
                .Where(s => s.IsActive != false && s.Action != (int)DatabaseAction.Delete)
                .Where(p => p.Responsible.Upper().Contains(this._UID) || p.CoResponsibles != null ? p.CoResponsibles.Upper().Contains(this._UID) : false)
                .Select(s => new EliteTask
                {
                    Id = s.Id,
                    Title = s.Title,
                    Description = s.Description,
                    DueDate = s.DueDate,
                    Responsible = s.Responsible,
                    CoResponsibles = s.CoResponsibles,
                    Status = s.Status,
                    CreatedBy = s.CreatedBy,
                    ParentId = s.ParentId,
                    IsPublishedToJira = s.IsPublishedToJira,
                    JiraTicketInfo = s.JiraTicketInfo
                })
                , new OrderByExpression<EliteTask, DateTime?>(p => p.DueDate.Value, false),
                      new OrderByExpression<EliteTask, string>(p => p.Title, false))
                .Take(11)
                .AsNoTracking()
                .ToListAsync();

        }
        private PaginationMetadata getPageMetaData<T>(PaginatedList<T> topics) where T : class
        {
            return new PaginationMetadata
            {
                PageIndex = topics.PageIndex,
                TotalPages = topics.TotalPages,
                HasNextPage = topics.HasNextPage,
                HasPreviousPage = topics.HasPreviousPage
            };
        }
        private TaskDueStatus? GetTaskDueStatus(DateTime dueDate)
        {
            TimeSpan t = dueDate - DateTime.Now;
            double noOfDays = t.TotalDays;
            if (noOfDays < 0) return TaskDueStatus.Overdue;

            //Currently we have hard coded for one week (noOfDays <= 7), this information will be configured in committee  lavel
            if (noOfDays <= 7) return TaskDueStatus.ApproachingDueDate;
            return (TaskDueStatus?)null;
        }
        private bool IsNullCheck(object obj)
        {
            return obj is null;
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

        private string GetCommitteePoolIdEmail(long? committeeId)
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

        private IList<QueriesTaskListDto> GetTaskListDtos(PaginatedList<EliteTask> tasks)
        {
            if (this.rolesPermissions.IsAdmin || this.rolesPermissions.IsTransient)
                return (from p in tasks
                        join y in this.committees on p.CommitteeId equals y.CommitteeId into joinedY
                        from y in joinedY.DefaultIfEmpty()
                        select new QueriesTaskListDto
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                            ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                            CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                            Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                            TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                            HasSubTask = (p.SubTaskCount > 1) ? true : false,
                            SubTaskCount = (p.SubTaskCount - 1),
                            HasMeetingTask = p.MeetingId.HasValue,
                            MeetingId = p.MeetingId,
                            MeetingDate = p.MeetingDate,
                            CommitteeName = (y != null) ? y.CommitteeName : string.Empty,
                            CommitteeId = (y != null) ? y.CommitteeId : (long?)null,
                            MeetingStatus = p.MeetingStatus,
                            AgendaId = p.AgendaId,
                            CreatedByJson = JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy),
                            Actions = this.rolesPermissions.IsAdmin ? _configuration.GetSection("ContextMenuFullPermissions:Actions").Get<List<EntityAction>>() : _configuration.GetSection("ContextMenuLessPermissions:Actions").Get<List<EntityAction>>(),
                            IsPublishedToJira = p.IsPublishedToJira,
                            JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                            ResponsibleDivision = p.ResponsibleDivision
                        }).ToList();
            else
                return (from p in tasks
                        join ro in this.rolesPermissions.UserRolesAndRights on p.CommitteeId equals ro.CommitteeId into joinedV
                        from ro in joinedV.DefaultIfEmpty()
                        join y in this.committees on p.CommitteeId equals y.CommitteeId into joinedY
                        from y in joinedY.DefaultIfEmpty()
                        select new QueriesTaskListDto
                        {
                            Id = p.Id,
                            Title = p.Title,
                            Description = p.Description,
                            DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                            ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                            CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                            Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                            TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                            HasSubTask = (p.SubTaskCount > 1) ? true : false,
                            SubTaskCount = (p.SubTaskCount - 1),
                            HasMeetingTask = p.MeetingId.HasValue,
                            MeetingId = p.MeetingId,
                            MeetingDate = p.MeetingDate,
                            CommitteeName = (y != null) ? y.CommitteeName : string.Empty,
                            CommitteeId = GetCommitteeIDbyRoles(ro != null ? ro.RoleId : (int?)null, (y != null) ? y.CommitteeId : (long?)null),
                            MeetingStatus = p.MeetingStatus,
                            AgendaId = p.AgendaId,
                            CreatedByJson = JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy),
                            Actions = GetUserAction(this.rolesPermissions.IsCmCoMUser, p.MeetingId, p.CreatedBy, ro != null ? ro.RoleId : (int?)null),
                            IsPublishedToJira = p.IsPublishedToJira,
                            JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                            ResponsibleDivision = p.ResponsibleDivision
                        }).ToList();
        }

        private long? GetCommitteeIDbyRoles(int? roleId, long? committeeID)
        {
            if (roleId.HasValue &&
                (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.CoreMember ||
                roleId.Value == (int)RolesType.User || roleId.Value == (int)RolesType.AssistantDocumentMember) && committeeID.HasValue)
            {
                return committeeID;
            }
            return (long?)null;
        }

        private async Task<QueriesTaskDto> GetTaskDto(long id)
        {
            if (this.rolesPermissions.IsAdmin)
            {
                return await (from p in this.GetDBSet<EliteTask>()
                                      .Include(EliteTask.PATH_EliteTaskAttachment)
                                      .Include(EliteTask.PATH_EliteSubTask)
                                      .Include(EliteTask.PATH_EliteTaskComment)
                              where p.Id.Equals(id)
                              let commentCount = p.TaskComment != null ? (int)p.TaskComment.Count : 0
                              select new QueriesTaskDto
                              {
                                  Id = p.Id,
                                  Title = p.Title,
                                  Description = p.Description ?? string.Empty,
                                  ClosureComment = p.ClosureComment ?? string.Empty,
                                  DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                  Responsible = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                  CoResponsible = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                  IsCustomEmailRecipient = p.IsCustomEmailRecipient,
                                  CoResponsibleEmailRecipient = p.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibleEmailRecipient) : null,
                                  ResponsibleEmailRecipient = p.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ResponsibleEmailRecipient) : null,
                                  Status = p.Status,
                                  FileLink = p.FileLink,
                                  Attachments = p.TaskAttachmentMapping.Where(r => r.TaskId.Value.Equals(id)).Select(x => GetAttachment(x)),
                                  SubTask = p.InverseParent.Select(x => GetSubTask(p.CreatedBy.Upper(), p.Responsible.Upper(), GetRole(), x,
                                                                              (x.TaskComment != null ? (int)x.TaskComment.Count : 0),
                                                                              p.MeetingId.HasValue)),
                                  CreatedBy = Convert.ToString(p.CreatedBy.ToString()) != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy) : null,
                                  CommitteeId = p.CommitteeId,
                                  MeetingId = p.MeetingId,
                                  CommitteeName = GetCommitteeName(p.CommitteeId) ?? string.Empty,
                                  CommentCount = commentCount,
                                  IsFinalMinutesTasks = p.IsFinalMinutesTasks,
                                  MeetingStatus = p.MeetingStatus,
                                  //change by vimal
                                  notifyUsers = p.IsNotify,
                                  IsPublishedToJira = p.IsPublishedToJira,
                                  JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                                  Actions = GetEntityActions(),
                                  ResponsibleDivision = p.ResponsibleDivision
                              }).AsQueryable()
                                .AsNoTracking()
                                .SingleOrDefaultAsync();

            }
            else if (this.rolesPermissions.IsTransient)
            {
                return await (
                            from p in this.GetDBSet<EliteTask>().Include(EliteTask.PATH_EliteTaskAttachment)
                                                 .Include(EliteTask.PATH_EliteSubTask)
                                                 .Include(EliteTask.PATH_EliteTaskComment)
                            let substask = p.InverseParent
                            let commentCount = p.TaskComment != null ? (int)p.TaskComment.Count : 0
                            where p.Id.Equals(id)
                            select new QueriesTaskDto
                            {
                                Id = p.Id,
                                Title = p.Title,
                                Description = p.Description ?? string.Empty,
                                ClosureComment = p.ClosureComment ?? string.Empty,
                                DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                Responsible = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                CoResponsible = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                IsCustomEmailRecipient = p.IsCustomEmailRecipient,
                                CoResponsibleEmailRecipient = p.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibleEmailRecipient) : null,
                                ResponsibleEmailRecipient = p.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ResponsibleEmailRecipient) : null,
                                Status = p.Status,
                                FileLink = p.FileLink,
                                Attachments = p.TaskAttachmentMapping.Where(r => r.TaskId.Value.Equals(id)).Select(x => GetAttachment(x)),
                                SubTask = substask.Where(x => IsSubTaskAvailable(x.CreatedBy, x.Responsible, p.CreatedBy, p.Responsible, GetRole()))
                                                  .Select(x => GetSubTask(p.CreatedBy.Upper(), p.Responsible.Upper(), GetRole(), x, 
                                                  (x.TaskComment != null ? (int)x.TaskComment.Count : 0),
                                                                          p.MeetingId.HasValue)),
                                CreatedBy = Convert.ToString(p.CreatedBy.ToString()) != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy) : null,
                                CommitteeId = p.CommitteeId,
                                MeetingId = p.MeetingId,
                                CommitteeName = GetCommitteeName(p.CommitteeId) ?? string.Empty,
                                CommentCount = commentCount,
                                IsFinalMinutesTasks = p.IsFinalMinutesTasks,
                                MeetingStatus = p.MeetingStatus,
                                notifyUsers = p.IsNotify,
                                Actions = GetUserAction(p.CreatedBy.Upper(), p.Responsible.Upper(), p.CoResponsibles != null ? p.CoResponsibles.Upper() : "", substask.Any(s => p.Responsible.Upper().Contains(this._UID.Upper())), (int?)RolesType.Transient, p.MeetingId.HasValue),
                                IsPublishedToJira = p.IsPublishedToJira,
                                JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                                ResponsibleDivision = p.ResponsibleDivision

                            }).AsQueryable()
                         .AsNoTracking()
                         .SingleOrDefaultAsync();
            }
            else
            {
                var result = await (
                            from p in this.GetDBSet<EliteTask>().Include(EliteTask.PATH_EliteTaskAttachment)
                                                 .Include(EliteTask.PATH_EliteSubTask)
                                                 .Include(EliteTask.PATH_EliteTaskComment)
                            let roles = p.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(p.CommitteeId)) : null
                            let roleId = roles != null ? roles.RoleId : (int?)null
                            let substask = p.InverseParent
                            let commentCount = p.TaskComment != null ? (int)p.TaskComment.Count : 0
                            where p.Id.Equals(id)
                            select new QueriesTaskDto
                            {
                                Id = p.Id,
                                Title = p.Title,
                                Description = p.Description ?? string.Empty,
                                ClosureComment = p.ClosureComment ?? string.Empty,
                                DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                Responsible = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                CoResponsible = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                IsCustomEmailRecipient = p.IsCustomEmailRecipient,
                                CoResponsibleEmailRecipient = p.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibleEmailRecipient) : null,
                                ResponsibleEmailRecipient = p.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ResponsibleEmailRecipient) : null,
                                Status = p.Status,
                                FileLink = p.FileLink,
                                Attachments = p.TaskAttachmentMapping.Where(r => r.TaskId.Value.Equals(id)).Select(x => GetAttachment(x)),
                                SubTask = substask.Where(x => IsSubTaskAvailable(x.CreatedBy, x.Responsible, p.CreatedBy, p.Responsible, roleId))
                                                  .Select(x => GetSubTask(p.CreatedBy != null ? p.CreatedBy.Upper() : null,
                                                                          p.Responsible != null ? p.Responsible.Upper() : null,
                                                                          roleId, x, (x.TaskComment != null ? (int)x.TaskComment.Count : 0),
                                                                          p.MeetingId.HasValue)),
                                CreatedBy = p.CreatedBy != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy) : null,
                                CommitteeId = p.CommitteeId,
                                MeetingId = p.MeetingId,
                                CommitteeName = GetCommitteeName(p.CommitteeId) ?? string.Empty,
                                CommentCount = commentCount,
                                IsFinalMinutesTasks = p.IsFinalMinutesTasks,
                                MeetingStatus = p.MeetingStatus,
                                notifyUsers = p.IsNotify,
                                Actions = GetUserAction(p.CreatedBy != null ? p.CreatedBy.Upper() : null,
                                p.Responsible != null ? p.Responsible.Upper() : null,
                                p.CoResponsibles != null ? p.CoResponsibles.Upper() : "", substask.Any(s => s.Responsible.Upper().Contains(this._UID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, p.MeetingId.HasValue),
                                IsPublishedToJira = p.IsPublishedToJira,
                                JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                                RoleId = roleId,
                                ResponsibleDivision = p.ResponsibleDivision
                            }).AsQueryable()
                         .AsNoTracking()
                         .SingleOrDefaultAsync();

                if (result?.Actions != null)
                {
                    CheckPermissionsWithAction(result.Actions.ToList(), TaskEntityActionType.ViewTask);
                }
                return result;
            }
        }

        private IList<EntityAction> GetEntityActions()
        {
            if (this.rolesPermissions.IsAdmin)
                return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
            //else if (_isTransient)
            //    return _configuration.GetSection("TransientPermissions: Actions").Get<List<EntityAction>>();
            return null;
        }

        private bool IsSubTaskAvailable(string subTaskCreatedby, string subTaskResponsible, string createdby, string responsible, int? roleId)
        {
            return (
                      (
                           roleId.HasValue && roleId.Value == (int)RolesType.CommitteeManager
                        || roleId.HasValue && roleId.Value == (int)RolesType.CoreMember ? true : false
                       )
                        || createdby.Upper().Contains(this._UID) || responsible.Upper().Contains(this._UID)
                        || subTaskCreatedby.Upper().Contains(this._UID) || subTaskResponsible.Upper().Contains(this._UID)
                   );
        }

        private static void CheckPermissionsWithAction(List<EntityAction> actions, TaskEntityActionType action)
        {
            RolePermissions rolePermissions = new RolePermissions();
            if (!rolePermissions.ValidateActionType(actions.ToList(), action))
            {
                throw new EliteException($"unauthorized");
            }
        }

        #endregion

        public string GetFileNameforExcel()
        {
            string fileName = "";
            fileName += "ListToEXCEL" + DateTime.Now.ToString("DDMMYYYYHHmmss") + ".xlsx";
            return fileName;
        }

        #region EXCEL Download
        public async Task<IActionResult> GetTasksForEXCEL(int? pageIndex, TaskSearchKeywords taskSearchKeywords, FilterActionEnum taskFilterAction, HttpResponse response)
        {
            List<QueriesTaskListDto> tasks = new List<QueriesTaskListDto>();
            tasks = await GetTasksforDownload(pageIndex, taskSearchKeywords, taskFilterAction);
            ExcelDownload ex = new ExcelDownload();
            List<ListToDownloadDto> listToDownloadDto = new List<ListToDownloadDto>();
            string tableName = "Task List";
            List<string> columnNames = new List<string>();
            columnNames.Add("Task Name");
            columnNames.Add("Description");
            columnNames.Add("Responsible");
            columnNames.Add("Co-Responsible");
            columnNames.Add("Committee");
            columnNames.Add("Status");
            columnNames.Add("Due Date");
            columnNames.Add("File Link");
            columnNames.Add("Closure Comment");
            columnNames.Add("Comment");
            columnNames.Add("Commented By");
            columnNames.Add("Commented On");
            columnNames.Add("Meeting Date");
            columnNames.Add("Division (Responsible)");

            foreach (var item in tasks)
            {
                if(item != null)
                {
                    var listItem = mapItems(item);

                    var queryComments = await _taskQueries.GetCommentsByTaskID(item.Id);
                    listItem.TaskComments = queryComments.Select(c => new TaskCommentDto
                    {
                        Comment = c.Comment,
                        CreatedBy = c.CreatedBy.FullName?.ToString() ?? string.Empty,
                        CreatedDate = c.CreatedDate
                    }).Reverse().ToList();

                    listToDownloadDto.Add(listItem);
                }
            }

            var res = await ex.DownloadsSpreadsheet(listToDownloadDto, tableName, columnNames);

            return new FileStreamResult(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = GetFileNameforExcel()
            };
        }
        public ListToDownloadDto mapItems(QueriesTaskListDto tasks)
        {
            ListToDownloadDto list = new ListToDownloadDto();
            list.Title = tasks.Title;
            list.Description = tasks.Description;
            list.ResponsibleJson.DisplayName = tasks.ResponsibleJson.DisplayName;
            if (tasks.CoResponsibleJson != null)
                foreach (var coresp in tasks.CoResponsibleJson)
                    list.CoResponsibleJson.Add(mapCoResponsibleJson(coresp));
            list.CommitteeName = tasks.CommitteeName;
            list.DueDate = tasks.DueDate.Value.ToString("dd.MM.yyyy");
            list.Status = tasks.Status.ToString();
            list.taskDueStatus = tasks.TaskDueStatus == TaskDueStatus.Overdue ? "OverDue" : "Approaching Due date";
            list.Filelink = tasks.FileLink;
            list.TaskClosureComment = tasks.ClosureComment ?? string.Empty;
            list.ResponsibleDivision = tasks.ResponsibleDivision;
            list.MeetingDate = tasks.MeetingDate;
            list.ResponsibleDivision = tasks.ResponsibleDivision;
            return list;


        }
        public QuerieGroupDto mapCoResponsibleJson(QueriesGroupDto coresp)
        {
            QuerieGroupDto responsible = new QuerieGroupDto();
            responsible.DisplayName = coresp.DisplayName;
            responsible.Uid = coresp.Uid;
            if (coresp.Users != null)
            {
                foreach (var user in coresp.Users)
                {
                    PersonDto person = new PersonDto();
                    person.Uid = user.Uid;
                    person.DisplayName = user.DisplayName;
                    responsible.Users.Add(person);

                }

            }

            return responsible;
        }
        #endregion

        #region PDF Download Functionality
        public async Task<IActionResult> GetPdfData(int? pageIndex, TaskSearchKeywords taskSearchKeywords, FilterActionEnum taskFilterAction, HttpResponse response)
        {
            List<QueriesTaskListDto> lstQueriesTaskList = new List<QueriesTaskListDto>();
            lstQueriesTaskList = await GetTasksforDownload(pageIndex, taskSearchKeywords, taskFilterAction);

            string pdfpath = _configuration.GetSection("filePdfPathTemplate").Value;
            HtmlDocument document = new HtmlDocument();
            document.Load(AppDomain.CurrentDomain.BaseDirectory + "task-list-pdf.html");
            StringBuilder stringPDFData = new StringBuilder();
            string s = document.Text;
            stringPDFData.Append(s);

            if (taskSearchKeywords.CommitteeId?.Count > 0)
            {
                string committeeNameFiltered = string.Empty;
                foreach (var cmte in taskSearchKeywords.CommitteeId)
                {
                    committeeNameFiltered += this.committees.Where(c => c.CommitteeId == cmte).Select(p => p.CommitteeName).FirstOrDefault() + ",";
                }
                stringPDFData.Replace("[CommitteeFilter]", committeeNameFiltered.TrimEnd(','));
            }
            else
            {
                stringPDFData.Replace("[CommitteeFilter]", "");
            }

            if (taskSearchKeywords.Responsible.Count > 0)
            {
                string responsibleData = string.Empty;
                foreach (var responsibleJson in taskSearchKeywords.Responsible)
                {
                    responsibleData += responsibleJson + ",";
                }
                stringPDFData.Replace("[AssignedToFilter]", responsibleData.TrimEnd(','));
            }
            else
            {
                stringPDFData.Replace("[AssignedToFilter]", "");
            }
            string statusText = string.Empty;
            if (taskSearchKeywords.TaskType[0])
            {
                statusText = (taskSearchKeywords.TaskType[0]) ? "Assigned" + "," : "";
            }
            if (taskSearchKeywords.TaskType[1])
            {
                statusText += (taskSearchKeywords.TaskType[1]) ? " InProgress" + "," : "";
            }
            if (taskSearchKeywords.TaskType[2])
            {
                statusText += (taskSearchKeywords.TaskType[2]) ? " Completed" + "," : "";
            }

            stringPDFData.Replace("[StatusFilter]", statusText == "" ? " " : statusText.TrimEnd(','));

            if (taskSearchKeywords.DueStartDate != null || taskSearchKeywords.DueEndDate != null)
            {
                string dateData = string.Empty;
                if (taskSearchKeywords.DueStartDate != null)
                {
                    dateData = ((DateTime)taskSearchKeywords.DueStartDate).ToString("dd.MM.yyyy");
                }
                if (taskSearchKeywords.DueEndDate != null)
                {
                    dateData += " - " + ((DateTime)taskSearchKeywords.DueEndDate).ToString("dd.MM.yyyy");
                }
                stringPDFData.Replace("[DateFilter]", dateData);
            }
            else
            {
                stringPDFData.Replace("[DateFilter]", "");
            }

            foreach (var item in lstQueriesTaskList)
            {
                stringPDFData.Append("<div class='content-block'>");
                stringPDFData.Append("<div class='row list-table-row'>");
                stringPDFData.Append("<div class='col-2 wrap-text'>");
                stringPDFData.Append(item.Title);
                stringPDFData.Append("</div>");

                stringPDFData.Append("<div class='col-2 wrap-text'>");
                stringPDFData.Append(item.ResponsibleJson.DisplayName);
                stringPDFData.Append("</div>");


                stringPDFData.Append("<div class='col-2  wrap-text'>");
                if (item.CoResponsibleJson != null)
                    foreach (var coresp in item.CoResponsibleJson)
                    {
                        stringPDFData.Append(coresp.DisplayName);
                        stringPDFData.Append("</br>");

                    }
                stringPDFData.Append("</div>");



                stringPDFData.Append("<div class='col-2 wrap-text'>");
                stringPDFData.Append(item.CommitteeName);
                stringPDFData.Append("</div>");


                stringPDFData.Append("<div class='col-2 wrap-text'>");
                stringPDFData.Append(item.Status);
                if (item.TaskDueStatus != null)
                    stringPDFData.Append(" (" + item.TaskDueStatus + ")");
                stringPDFData.Append("</div>");
                stringPDFData.Append("<div class='col-2 wrap-text'>");
                stringPDFData.Append(item.DueDate.Value.ToString("dd.MM.yyyy"));
                stringPDFData.Append("</div>");


                stringPDFData.Append("</div>");
                stringPDFData.Append("</div>");

                stringPDFData.Append("<div class='content-block'>");
                stringPDFData.Append("<div class='row list-table-row'>");
                stringPDFData.Append("<div class='col-12 wrap-text'>");
                stringPDFData.Append($"Closure Comment: {item.ClosureComment ?? String.Empty}");
                stringPDFData.Append("</div>");
                stringPDFData.Append("</div>");
                stringPDFData.Append("</div>");
                stringPDFData.Append("<hr>");

            }
            stringPDFData.Append("</section>");

            stringPDFData.Append("</section>");
            stringPDFData.Append("</div>");
            stringPDFData.Append("</div>");
            stringPDFData.Append("</body>");
            stringPDFData.Append("</html>");
            string fileName = await GetFileName();

            var isWatermarkEnabled = Convert.ToBoolean(_configuration.GetSection("ConnectionConfiguration:enableWatermarkinPDF").Value);
            var isDateinWatermarkEnabled = Convert.ToBoolean(_configuration.GetSection("ConnectionConfiguration:enableDateinWatermark").Value);
            var isUserNameinWatermarkEnabled = Convert.ToBoolean(_configuration.GetSection("ConnectionConfiguration:enableUserNameinWatermark").Value);
            var textWatermark = _configuration.GetSection("ConnectionConfiguration:textWatermark").Value;



            ConvertHtmlToPDF convertHtmlToPDF = new ConvertHtmlToPDF(_converter);
            string pdfhtmlpath = convertHtmlToPDF.StartConvert(stringPDFData.ToString(), fileName, _configuration);

            // string pdfhtmlpath = convertHtmlToPDF.NewConvert(stringPDFData.ToString(), fileName, _configuration).Result.ToString();

            if (Convert.ToBoolean(isWatermarkEnabled))
            {
                var usrInfo = await _userService.GetUserInfos((_requestContext.DeputyUID is null ? _requestContext.UID : _requestContext.DeputyUID));

                string userName = (isUserNameinWatermarkEnabled ? usrInfo.DisplayName : "");
                string dtNow = isDateinWatermarkEnabled ? " " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") : "";
                pdfhtmlpath = convertHtmlToPDF.AddWaterMarkPDF(pdfhtmlpath, textWatermark, userName, dtNow);
            }


            byte[] bytes = System.IO.File.ReadAllBytes(pdfhtmlpath);
            var pdfName = pdfhtmlpath.Substring(pdfhtmlpath.LastIndexOf('\\') + 1);
            MemoryStream stream = new MemoryStream(bytes);
            var contentType = string.Empty;
            new FileExtensionContentTypeProvider().TryGetContentType(pdfName, out contentType);

            try
            {
                if (File.Exists(pdfhtmlpath))
                {
                    File.Delete(pdfhtmlpath);
                }
            }
            catch (Exception ex)
            {
                //No need to report this, as even if it fails to remove, it will overwrite next time
                //Skip the error.
            }
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = pdfName
            };
        }
        public async Task<string> GetFileName()
        {
            string fileName = "";
            fileName += "ListTOPDF" + DateTime.Now.ToString("DDMMYYYYHHmmss") + ".pdf";
            return fileName;
        }
        #endregion

        #region get task list for pdf and excel 
        public async Task<List<QueriesTaskListDto>> GetTasksforDownload(int? page, TaskSearchKeywords searchKeywords, FilterActionEnum topicFilterAction)
        {
            ITaskRolesAndRightFilterBuilder<EliteTask> rolesBuilder = TaskRolesAndRightsFilterFactory.GetTaskRolesAndRightsFilterObject(topicFilterAction, this.rolesPermissions.UserRolesAndRights, this._requestContext, isEliteClassic);
            if (rolesBuilder is null)
                throw new EliteException($"{nameof(ITaskRolesAndRightFilterBuilder<EliteTask>)} should not be empty");

            var builder = new TaskFilterCollectionBuilder<EliteTask>(searchKeywords);
            var task = (await GetTaskByPaginate(builder.BuildFilter(), this.rolesPermissions.IsAdmin ? null : rolesBuilder.BuildFilter(), searchKeywords)).OrderBy(c => c.Status);
            int pageSize = task.Count();
            var tasks = PaginatedList<EliteTask>.Create(builder.ApplyOrderBy(task), page ?? 1, pageSize);

            List<QueriesTaskListDto> lstQueriesTaskList = new List<QueriesTaskListDto>();

            if (tasks?.Count() > 0)
            {
                if (this.rolesPermissions.IsAdmin)
                {
                    lstQueriesTaskList = (from p in tasks
                                          select new QueriesTaskListDto
                                          {
                                              Id = p.Id,
                                              Title = p.Title,
                                              Description = p.Description,
                                              DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                              ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                              ResponsibleEmailRecipientJson = p.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ResponsibleEmailRecipient) : null,
                                              CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                              CoResponsibleEmailRecipientJson = p.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibleEmailRecipient) : null,
                                              Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                                              TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                                              HasMeetingTask = p.MeetingId.HasValue,
                                              MeetingId = p.MeetingId,
                                              MeetingStatus = p.MeetingStatus,
                                              FileLink = p.FileLink,
                                              IsPublishedToJira = p.IsPublishedToJira,
                                              JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                                              ClosureComment = p.ClosureComment ?? String.Empty,
                                              IsCustomEmailRecipient = p.IsCustomEmailRecipient,
                                              ResponsibleDivision = p.ResponsibleDivision,
                                              MeetingDate = p.MeetingDate.HasValue ? p.MeetingDate.Value : (DateTime?)null
                                          }).ToList();
                }
                else
                {
                    lstQueriesTaskList = (from p in tasks
                                          join ro in this.rolesPermissions.UserRolesAndRights on p.CommitteeId equals ro.CommitteeId into joinedV
                                          from v in joinedV.DefaultIfEmpty()
                                          join y in this.committees on p.CommitteeId equals y.CommitteeId into joinedY
                                          from y in joinedY.DefaultIfEmpty()
                                          select new QueriesTaskListDto
                                          {
                                              Id = p.Id,
                                              Title = p.Title,
                                              Description = p.Description,
                                              DueDate = p.DueDate.HasValue ? p.DueDate.Value : (DateTime?)null,
                                              ResponsibleJson = p.Responsible != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.Responsible) : null,
                                              ResponsibleEmailRecipientJson = p.ResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ResponsibleEmailRecipient) : null,
                                              CoResponsibleJson = p.CoResponsibles != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibles) : null,
                                              CoResponsibleEmailRecipientJson = p.CoResponsibleEmailRecipient != null ? JsonConvert.DeserializeObject<List<QueriesGroupDto>>(p.CoResponsibleEmailRecipient) : null,
                                              Status = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status,
                                              TaskDueStatus = (Elite_Task.Microservice.CommonLib.TaskStatus)p.Status == CommonLib.TaskStatus.Completed ? (TaskDueStatus?)null : p.DueDate.HasValue ? GetTaskDueStatus(p.DueDate.Value) : (TaskDueStatus?)null,
                                              HasMeetingTask = p.MeetingId.HasValue,
                                              MeetingId = p.MeetingId,
                                              MeetingStatus = p.MeetingStatus,
                                              CommitteeName = (y != null) ? y.CommitteeName : string.Empty,
                                              FileLink = p.FileLink,
                                              IsPublishedToJira = p.IsPublishedToJira,
                                              JiraTicketInfo = p.JiraTicketInfo != null ? JsonConvert.DeserializeObject<QueriesJiraTicketInfoDto>(p.JiraTicketInfo) : null,
                                              ClosureComment = p.ClosureComment ?? String.Empty,
                                              IsCustomEmailRecipient = p.IsCustomEmailRecipient,
                                              ResponsibleDivision = p.ResponsibleDivision,
                                              MeetingDate = p.MeetingDate.HasValue ? p.MeetingDate.Value : (DateTime?)null
                                          }).ToList();
                }

            }
            return lstQueriesTaskList;
        }

        public async Task<List<AttachmentMappingDetails>> GetTaskAttachments()
        {
            return _context.vwAllTaskatttachments
                           .Select(x => new AttachmentMappingDetails()
                           {
                               fileName = x.AttachmentGUID.ToString(),
                               TaskName = x.TaskTitle,
                               TaskComment = x.TaskComment,
                           })
                           .ToList();
        }


        #endregion

        #region Get Task Overdue Summary Async
        public async Task<IList<TaskOverdueSummary>> GetTaskOverdueSummaryAsync(string committees, string divisions, string startDate, string endDate)
        {
            try
            {
                DateTime? start = null;
                DateTime? end = null;

                if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStart))
                    start = parsedStart;
                if (DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEnd))
                    end = parsedEnd;

                string formattedStart = start.HasValue ? $"'{start.Value:yyyy-MM-dd}'" : "NULL";
                string formattedEnd = end.HasValue ? $"'{end.Value:yyyy-MM-dd}'" : "NULL";
                string formattedCommittees = string.IsNullOrWhiteSpace(committees) ? "NULL" : $"'{committees}'";
                string formattedDivisions = string.IsNullOrWhiteSpace(divisions) ? "NULL" : $"'{divisions}'";

                string sql = $@"SELECT * FROM elite.get_task_overdue_summary_by_division({formattedCommittees}, {formattedDivisions}, {formattedStart}, {formattedEnd})";

                var data = await _context.get_task_overdue_summary.FromSql(sql).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region Get Division
        public async Task<IList<Division>> GetDivisionAsync(long[] committeeIds)
        {
            if (committeeIds == null || committeeIds.Length == 0)
                return new List<Division>();

            var result = await GetDBSet<EliteTask>()
                .AsNoTracking()
                .Where(s => s.Status != 3
                            && s.ParentId == null
                            && !string.IsNullOrWhiteSpace(s.ResponsibleDivision)
                            && committeeIds.Cast<long?>().Contains(s.CommitteeId))
                .GroupBy(s => s.ResponsibleDivision)
                .Select(g => new Division
                {
                    Label = g.Key,
                    Value = g.Key
                })
                .ToListAsync();

            return result;
        }
        #endregion

        public async Task<IList<string>> GetDepartment(string dep)
        {
            var data = await _context.EliteTask
                .Where(x => x.ResponsibleDivision != null && x.ResponsibleDivision.ToUpper().Contains(dep.ToUpper()))
                .Select(x => x.ResponsibleDivision)
                .ToListAsync();
            return data.Distinct().ToList();
        }

        #region Get Task line Chart Data
        public async Task<IList<CommitteeTaskSeries>> GetTaskLineChartDataAsync(string committees, string divisions, string startDate, string endDate)
        {
            try
            {
                DateTime? start = null;
                DateTime? end = null;

                if (DateTime.TryParseExact(startDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStart))
                    start = parsedStart;
                if (DateTime.TryParseExact(endDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEnd))
                    end = parsedEnd;


                List<DailyOpenTask> data;
                using (var conn = _context.Database.GetDbConnection())
                {
                    await conn.OpenAsync();
                    using (var cmd = new NpgsqlCommand("SELECT * FROM elite.get_committee_open_tasks_by_division(@start, @end, @committee_ids_text, @division_text)", (NpgsqlConnection)conn))
                    {
                        // Pass DBNull if no value
                        cmd.Parameters.AddWithValue("start", start.HasValue ? (object)start.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("end", end.HasValue ? (object)end.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("committee_ids_text", string.IsNullOrWhiteSpace(committees) ? (object)DBNull.Value : committees);
                        cmd.Parameters.AddWithValue("division_text", string.IsNullOrWhiteSpace(divisions) ? (object)DBNull.Value : divisions);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var results = new List<DailyOpenTask>();
                            while (await reader.ReadAsync())
                            {
                                results.Add(new DailyOpenTask
                                {
                                    TaskDate = reader.GetDateTime(0),
                                    CommitteeId = reader.GetInt64(1),
                                    Division = reader.IsDBNull(2) ? null : reader.GetString(2),
                                    OpenTasksCount = reader.GetInt32(3)
                                });
                            }

                            data = results;
                        }
                    }
                }

                // If division filter is provided -> group by Division; otherwise group by Committee
                var grouped = string.IsNullOrWhiteSpace(divisions)
                    ? data.GroupBy(x => x.CommitteeId)
                          .Select(g => new CommitteeTaskSeries
                          {
                              Committee = g.Key.ToString(),
                              Data = g.GroupBy(x => x.TaskDate)   // group by date inside each committee
                                      .OrderBy(x => x.Key)
                                      .Select(x => new ChartDataPoint
                                      {
                                          Date = x.Key.ToString("dd-MM-yyyy"),
                                          Values = x.Sum(y => y.OpenTasksCount).ToString()
                                      }).ToList()
                          }).ToList()
                    : data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Division))   //  filter out null/empty divisions
                        .GroupBy(x => x.Division)
                        .Select(g => new CommitteeTaskSeries
                        {
                            Committee = null,
                            Division = g.Key,
                            Data = g.GroupBy(x => x.TaskDate)   // group by date inside each division
                                    .OrderBy(x => x.Key)
                                    .Select(x => new ChartDataPoint
                                    {
                                        Date = x.Key.ToString("dd-MM-yyyy"),
                                        Values = x.Sum(y => y.OpenTasksCount).ToString()
                                    }).ToList()
                        })
                        .ToList();

                return grouped;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        public async Task<IList<LookUp>> GetListCMUserCommittees(string uid)
        {
            return await _userService.GetListofUsersCommitteeManagersCommitteeAsync(uid);
        }

        #region Get Global serach content
        public async Task<IList<GlobalSearchTaskDto>> GetAllTasksContentPaginated(
    int? page, int listType, string searchKeywords, HttpResponse response, FilterActionEnum taskFilterAction, string uid)
        {
            ITaskRolesAndRightFilterBuilder<EliteTask> rolesBuilder = TaskRolesAndRightsFilterFactory.GetTaskRolesAndRightsFilterObject(taskFilterAction, this.rolesPermissions.UserRolesAndRights, this._requestContext, isEliteClassic);


            if (rolesBuilder == null)
                throw new EliteException("Roles and rights filter must not be null");

            string committeeIds = string.Empty;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                var userCommittees = await GetAllUserCommitteesForTask(uid);
                committeeIds = string.Join(",", userCommittees.Select(c => c.Id));
            }

            int pageIndex = page ?? 1;
            int pageSize = 10;
            var (taskList, totalCount) = await GetAllTasksContentPaginatedQuery(
                pageIndex,
                pageSize,
                committeeIds,
                searchKeywords);

            var paginatedList = PaginatedList<GlobalSearchTaskDto>.CreateUpdatedAsync(
                taskList,
                pageIndex,
                pageSize,
                totalCount);

            response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(getPageMetaData(paginatedList)));

            foreach (var task in taskList)
                task.TotalTaskCount = totalCount;

            return taskList;
        }

        private async Task<Tuple<List<GlobalSearchTaskDto>, int>> GetAllTasksContentPaginatedQuery(
            int pageIndex, int pageSize, string rolesCommitteeIds, string searchTerm)
        {
            try
            {
                int resultCount = 0;

                var rawResults = await _context.GlobalSearchTaskEntity
                    .FromSql("SELECT * FROM elite.get_filtered_tasks_by_committee_and_keyword_1({0},{1}, 0.25, {2}, {3}, true, 'english')",
                        searchTerm, rolesCommitteeIds, pageSize, pageIndex)
                    .ToListAsync();

                int totalCount = rawResults.FirstOrDefault()?.total_results ?? 0;

                var taskList = rawResults.Select(r => new GlobalSearchTaskDto
                {
                    Id = r.task_id,
                    MeetingId = r.meeting_id,
                    AgendaId = r.agenda_id,
                    Committee = r.committee_id,
                    Title = r.title,
                    Description = r.description,
                    DescriptionWithoutHtml = r.description_without_html,
                    Status = r.status,
                    Action = r.action,
                    DueDate = r.due_date,
                    ResponsibleJson = r.responsible_json,
                    CoResponsibleJson = r.co_responsibles_json,
                    ParentId = r.parent_id,
                    ResponsibleDivision = r.responsible_division,
                    CoResponsibleDivisions = r.co_responsible_divisions,
                    ClosureComment = r.closure_comment,
                    CompletionDate = r.completion_date,
                    RelevanceScore = r.relevance_score,
                    MatchTypes = r.match_types,
                    TotalMatches = r.total_matches,
                    CommentMatches = r.comment_matches,
                    AttachmentMatches = r.attachment_matches,
                    HighlightedTitle = r.highlighted_title,
                    HighlightedDescription = r.highlighted_description,
                    HighlightedResponsibleJson = r.highlighted_responsible_json,
                    HighlightedCoResponsibleJson = r.highlighted_co_responsibles_json,
                    HighlightedClosureComment = r.highlighted_closure_comment,
                    MatchedCommentsPreview = r.matched_comments_preview,
                    MatchedAttachmentsPreview = r.matched_attachments_preview,
                    LanguageDetected = r.language_detected,
                    RowsCount = r.total_results,
                    PageNumber = r.page_number,
                    PageSize = r.page_size,
                    TotalPages = r.total_pages,
                    HasNextPage = r.has_next_page,
                    HasPreviousPage = r.has_previous_page
                }).ToList();

                return System.Tuple.Create(taskList, totalCount);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllTasksContentPaginatedQuery: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                }
                return System.Tuple.Create(new List<GlobalSearchTaskDto>(), 0);
            }
        }
        #endregion

    }








    class EqualityComparer : IEqualityComparer<QueriesTaskListDto>
    {
        public bool Equals(QueriesTaskListDto b1, QueriesTaskListDto b2)
        {
            if (b2 == null && b1 == null)
                return true;
            else if (b1 == null || b2 == null)
                return false;
            else if (b1.Id == b2.Id)
                return true;
            else
                return false;
        }

        public int GetHashCode(QueriesTaskListDto bx)
        {
            long hCode = bx.Id;
            return hCode.GetHashCode();
        }
    }




}

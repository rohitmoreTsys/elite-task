using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.CommonLib;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries
{
    public class TaskCommentQueries : BaseDataAccess, ITaskCommentQueries
    {
        private readonly IConfiguration _configuration;
        private readonly Func<IConfiguration, IRequestContext, IUserService> _userServiceFactory;
        private readonly IUserService _userService;
        private readonly IRequestContext _requestContext;
        private string _UID;
        private RolesAndRights<IUserRolesPermissions> rolesPermissions;

        public TaskCommentQueries(EliteTaskContext context, IConfiguration configuration, Func<IConfiguration, IRequestContext, IUserService> userServiceFactory, IRequestContext requestContext) : base(context)
        {
            _configuration = configuration;
            _userServiceFactory = userServiceFactory;
            _userService = userServiceFactory(configuration, requestContext);
            _requestContext = requestContext;
            rolesPermissions = new RolesAndRights<IUserRolesPermissions>(requestContext, _userService);
            this._UID = this._requestContext.IsDeputy ? _requestContext.DeputyUID.Upper() : _requestContext.UID.Upper();

        }
        public async Task<IList<QueriesCommentDto>> GetCommentsByTaskID(long taskId)
        {
            var result = await (from p in this.GetDBSet<EliteTask>().Include(EliteTask.PATH_EliteTaskAttachment)
                                                             .Include(EliteTask.PATH_EliteSubTask)
                                                             .Include(EliteTask.PATH_EliteTaskComment)
                                let roles = p.CommitteeId.HasValue ? this.rolesPermissions.UserRolesAndRights.SingleOrDefault(s => s.CommitteeId.Equals(p.CommitteeId)) : null
                                let roleId = roles != null ? roles.RoleId : (int?)null
                                let substask = p.InverseParent
                                let commentCount = (int)p.TaskComment.Count
                                where p.Id.Equals(taskId)
                                select new QueriesTaskDto
                                {
                                    Actions = GetUserAction(p.CreatedBy.Upper(), p.Responsible.Upper(), p.CoResponsibles != null ? p.CoResponsibles.Upper() : "", substask.Any(s => p.Responsible.Upper().Contains(this._UID)), roleId.HasValue ? roleId : (this.rolesPermissions.IsCmCoMUser) ? (int?)null : (int)RolesType.Transient, p.MeetingId.HasValue),
                                }).AsQueryable()
                          .AsNoTracking()
                          .SingleOrDefaultAsync();

            CheckPermissionsWithAction(result.Actions.ToList(), TaskEntityActionType.ViewTask);

            var tasks = await (from p in this.GetDBSet<TaskComment>()
                         .Include(TaskComment.PATH_EliteTaskCommentAttachment)
                               where p.TaskId.Value.Equals(taskId)
                               select new QueriesCommentDto
                               {
                                   Id = p.Id,
                                   TaskId = p.TaskId,
                                   Comment = p.Comment,
                                   CreatedBy = (!string.IsNullOrWhiteSpace(Convert.ToString(p.CreatedBy))) ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.CreatedBy) : null,
                                   CreatedDate = p.CreatedDate,
                                   ModifiedBy = (!string.IsNullOrWhiteSpace(Convert.ToString(p.ModifiedBy))) ? JsonConvert.DeserializeObject<QueriesPersonDto>(p.ModifiedBy) : null,
                                   ModifiedDate = p.ModifiedDate,
                                   Attachments = p.TaskCommentAttachmentMapping.Where(r => r.commentId.Equals(p.Id)).Select(x => GetAttachment(x))

                               })
                       .ToListAsync();
            return tasks.OrderByDescending(c => c.ModifiedDate.HasValue ? c.ModifiedDate : c.CreatedDate).ToList();

        }
        private QueriesTaskAttachmentDto GetAttachment(TaskCommentAttachmentMapping topicsAttachment)
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

        public async Task<List<string>> GetTaskAttachments(long taskCommentId)
        {
            return await (from p in _context.TaskCommentAttachmentMapping
                          where p.commentId == taskCommentId
                          select p.AttachmentGuid).ToListAsync();
        }

        private static void CheckPermissionsWithAction(List<EntityAction> actions, TaskEntityActionType action)
        {
            RolePermissions rolePermissions = new RolePermissions();
            if (!rolePermissions.ValidateActionType(actions.ToList(), action))
            {
                throw new EliteException($"unauthorized");
            }
        }

        private IList<EntityAction> GetUserAction(string createdby, string responsible, string coresponsible, bool hasSubTask, int? roleId, bool hasMeeting)
        {
            if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMemberExternal ||
                        roleId.Value == (int)RolesType.Guest))
            {
                if (((responsible != null && responsible.Contains(this._UID)) || (coresponsible != null && coresponsible.Contains(this._UID))) &&
                   (roleId.Value == (int)RolesType.Guest))
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
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.User))
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
}

using Elite.Common.Utilities.CommonType;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class RolePermissions
    {
        private bool isEliteClassic;

        public IList<EntityAction> GetUserAction(IConfiguration _configuration, string createdby, string responsible, string coresponsible, bool hasSubTask, int? roleId, bool hasMeeting, string UID)
        {
            isEliteClassic = _configuration.GetSection("isEliteClassic").Get<bool>();
            if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMemberExternal ||
                        roleId.Value == (int)RolesType.Guest))
            {
                if (isEliteClassic && ((createdby != null ? createdby.Contains(UID) : false) ||
                    (responsible != null ? responsible.Contains(UID) : false) ||
                    (coresponsible != null ? coresponsible.Contains(UID) : false)))
                {
                    return _configuration.GetSection("UserPartialPermissions:Actions").Get<List<EntityAction>>();
                }
                else
                {
                    return _configuration.GetSection("NoPermissions:Actions").Get<List<EntityAction>>();
                }
            }
            else if (roleId.HasValue && roleId.Value == (int)RolesType.User)
            {
                if (createdby.Contains(UID))
                {
                    return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
                }
                else if (!hasMeeting)
                {
                    if (responsible?.Contains(UID) == true || coresponsible?.Contains(UID) == true)
                    {
                        return _configuration.GetSection("UserPartialPermissions:Actions").Get<List<EntityAction>>();
                    }
                    else
                    {
                        return _configuration.GetSection("PartialFullPermissions:Actions").Get<List<EntityAction>>();
                    }
                }
                else
                {
                    return _configuration.GetSection("MeetingPartialFullPermissions:Actions").Get<List<EntityAction>>();
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
                {
                    if (isEliteClassic && ((createdby != null ? createdby.Contains(UID) : false) ||
                        (responsible != null ? responsible.Contains(UID) : false) ||
                        (coresponsible != null ? coresponsible.Contains(UID) : false)))
                    {
                        return _configuration.GetSection("CoreMemberPartialPermissions:Actions").Get<List<EntityAction>>();
                    }
                    else
                    {
                        return _configuration.GetSection("PartialFullPermissions:Actions").Get<List<EntityAction>>();
                    }
                }
                else
                    return _configuration.GetSection("MeetingPartialFullPermissions:Actions").Get<List<EntityAction>>();
            }
            else if (createdby.Contains(UID))
                return _configuration.GetSection("FullPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && roleId.Value == (int)RolesType.Transient && responsible.Contains(UID))
                return _configuration.GetSection("TaskResponsibleTransientPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && roleId.Value == (int)RolesType.Transient && (coresponsible != null ? coresponsible.Contains(UID) : false))
                return _configuration.GetSection("TaskResponsibleTransientPermissions:Actions").Get<List<EntityAction>>();
            else if (coresponsible != null ? coresponsible.Contains(UID) : false)
                return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
            else if (responsible.Contains(UID))
                return _configuration.GetSection("TransientPermissions:Actions").Get<List<EntityAction>>();
            else if (hasSubTask)
                return _configuration.GetSection("PartialPermissions:Actions").Get<List<EntityAction>>();
            else
                return _configuration.GetSection("NoPermissions:Actions").Get<List<EntityAction>>();
        }

        public IList<EntityAction> GetUserSubTaskAction(IConfiguration _configuration, int? roleId, List<TaskPersonCommand> subTaskResponsible, bool isMeetingTask, string UID)
        {
            if (roleId.HasValue && roleId.Value == (int)RolesType.Transient)
            {
                if (subTaskResponsible.Any(x => x.Uid.ToUpper() == UID.ToUpper()))
                    return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
            }
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.CommitteeManager || roleId.Value == (int)RolesType.Admin))
                return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMember))
                return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.User))
                return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.CoreMemberExternal || roleId.Value == (int)RolesType.Guest))
                return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();
            else if (roleId.HasValue && (roleId.Value == (int)RolesType.AssistantDocumentMember))
            {
                if (!isMeetingTask)
                    return _configuration.GetSection("SubTaskFullPermissions:Actions").Get<List<EntityAction>>();
                else
                    return _configuration.GetSection("SubTaskADMMeetingPermissions:Actions").Get<List<EntityAction>>();
            }
            if (subTaskResponsible.Any(x => x.Uid.ToUpper() == UID.ToUpper()))
                return _configuration.GetSection("SubTaskTransientPermissions:Actions").Get<List<EntityAction>>();
            else
                return _configuration.GetSection("SubTaskLeastPermissions:Actions").Get<List<EntityAction>>();

        }

        public bool ValidateActionType(List<EntityAction> actions, TaskEntityActionType actionType)
        {
            var action = actions.Where(y => y.InternalKey == (int)actionType).FirstOrDefault();
            return action != null ? action.IsAllowed : false;
        }

    }
}

using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.UserRolesAndPermissions;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.Application.CQRS.Helpers;
using Elite.Task.Microservice.CommonLib;
using Elite_Task.Microservice.Application.CQRS.Commands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
    public interface IUserService : IUserRolesPermissions
    {
        Task<List<CommitteeDetail>> GetCommitees();

        Task<UidEmail> GetUserEmailId(string id);

        Task<TaskPersonCommand> GetUser(string id);
        Task<CommitteeManagersMailIdsDto> GetCommitteeManagersMailIds(long id, string requestorUid, string createdByUid);

        LookUpFields GetUserDetail(string securedUID);

        Task<List<LookUp>> GetUserCommittees();

        Task<TaskPersonCommand> ValidateAndPostUsers(TaskPersonCommand jiraResponsible);

        Task<UserInfo> GetUserInfos(string id);

        Task<List<UserDeputies>> GetRestrictedUserDeputies(string userId);
        Task<List<LookUp>> GetListofUsersCommitteeManagersCommitteeAsync(string uid);
    }
}

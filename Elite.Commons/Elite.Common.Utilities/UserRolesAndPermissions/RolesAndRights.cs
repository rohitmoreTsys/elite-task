
using Elite.Common.Utilities.ExceptionHandling;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.UserRolesAndPermissions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Elite.Common.Utilities.CommonType
{
    public class RolesAndRights<T> where T : IUserRolesPermissions
    {

        private string _UID;
        private string _DeputyUID;
        private readonly T _userService;
        private readonly IRequestContext _requestContext;

        public IList<UserRolesAndRights> UserRolesAndRights { get; private set; }

        public bool IsAdmin { get; private set; } = false;
        public bool IsTransient { get; private set; } = false;

        public bool IsAnyUserRoles { get; private set; } = false;

        public bool IsCmCoMUser { get; private set; } = false;
        public bool IsAssistantDocumentMember { get; private set; } = false;

        public bool IsDeputy { get; private set; } = false;
        public long? CommitteeId { get; set; } = 0;

        public RolesAndRights(IRequestContext requestContext, T userService)
        {
            _requestContext = requestContext;
            _userService = userService;
            InitializeUsers();
        }
        private void InitializeUsers()
        {
            if (this._requestContext != null && !string.IsNullOrWhiteSpace(this._requestContext.UID))
            {
                this._UID = this._requestContext.UID.ToUpper();
                this._DeputyUID = !string.IsNullOrWhiteSpace(this._requestContext.DeputyUID)? this._requestContext.DeputyUID.ToUpper() : null;
                this.IsDeputy = !string.IsNullOrWhiteSpace(this._DeputyUID);

                Task.Run(async () =>
                {

                    if (this.IsDeputy)
                        this.UserRolesAndRights = await _userService.GetUserRolesAndRights(this._DeputyUID);
                    else
                        this.UserRolesAndRights = await _userService.GetUserRolesAndRights(this._UID);

                    if (this.UserRolesAndRights?.Count > 0)
                    {
                        IsAdmin = this.UserRolesAndRights.Min(p => p.RoleId).Equals((int)RolesType.Admin);
                        IsAnyUserRoles = this.UserRolesAndRights.Any(p => p.RoleId.Equals((int)RolesType.User));
                        IsCmCoMUser = this.UserRolesAndRights.Any(p => p.RoleId.Equals((int)RolesType.User) || p.RoleId.Equals((int)RolesType.CommitteeManager) || p.RoleId.Equals((int)RolesType.CoreMember));
                        IsAssistantDocumentMember = this.UserRolesAndRights.Any(p => p.RoleId.Equals((int)RolesType.AssistantDocumentMember));
                        if (IsAssistantDocumentMember)
                        {
                            CommitteeId = this.UserRolesAndRights.Where(p => p.RoleId.Equals((int)RolesType.AssistantDocumentMember)).ToList()[0].CommitteeId;
                        }

                        if (!this.IsAdmin)
                            this.IsTransient = this.UserRolesAndRights.Any(p => p.RoleId.Equals((int)RolesType.Transient));
                    }

                }).Wait();
            }
            //else 
            //    throw new EliteException($" { nameof(RequestContext.RequestContext) } should not be null ");
        }


        public int GetRoles()
        {
            if (this.UserRolesAndRights?.Count > 0)
            {
                var roleType = this.UserRolesAndRights.Min(p => p.RoleId);
                return roleType;
            }
            return (int)RolesType.Transient;
        }
    }
}

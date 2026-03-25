using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.CommonType
{
    public class UserRolesAndRights
    {
        public UserRolesAndRights()
        {
            Actions = new List<EntityAction>();
        }

        public string UID { get; set; }        
        public long? CommitteeId { get; set; }
        public string CommitteeName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<EntityAction> Actions { get; set; }
        public string CommitteeOwnCloudId { get; set; } = string.Empty;
        public bool? IsInternal { get; set; } = false;
    }
}

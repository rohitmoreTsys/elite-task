using Elite.Common.Utilities.CommonType;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.UserRolesAndPermissions
{
   public interface IUserRolesPermissions 
    {
       
        Task<IList<UserRolesAndRights>> GetUserRolesAndRights(string id);
    }
}

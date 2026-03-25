using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CommonLib
{
    public class UserExistAsResponsible
    {
        public static bool IsExist(EliteTask task, string UID)
        {
            var isResponsible = !string.IsNullOrWhiteSpace(Convert.ToString(task.Responsible)) ? task.Responsible.ToString().ToUpper().Contains(UID.ToUpper()) : false;
            return isResponsible;
        }
    }
}

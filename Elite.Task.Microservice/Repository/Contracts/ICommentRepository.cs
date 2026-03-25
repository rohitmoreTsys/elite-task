using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Core;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Repository.Contracts
{
    public interface ICommentRepository : ICommonRepository<TaskComment>, IRepository<TaskComment>
    {
        void Delete(List<string> GuiIDs);      
    }
}

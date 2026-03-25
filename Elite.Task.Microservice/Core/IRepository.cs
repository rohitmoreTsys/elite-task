using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elite_Task.Microservice.Core;

namespace Elite_Task.Microservice.Core
{
    public interface IRepository<T> where T : IEntity
    {
        IUnitOfWork UnitOfWork { get; }
    }
}

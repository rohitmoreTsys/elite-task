using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.SearchFilter
{
    public interface ITaskRolesAndRightFilterBuilder<T> where T : EliteTask, new()
    {
        Expression<Func<T, bool>> BuildFilter();
    }
}

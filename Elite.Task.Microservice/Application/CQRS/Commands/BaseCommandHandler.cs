
using Elite_Task.Microservice.CommonLib;
using Elite_Task.Microservice.Models.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public abstract class BaseCommandHandler<T> where T : class
    {           
              
        protected  bool IsUpdate(long Id)
        {
            return (Id > 0 ? true : false);
        }
        protected virtual bool IsCommitteeAdmin()
        {
            return false;
        }
    }

}

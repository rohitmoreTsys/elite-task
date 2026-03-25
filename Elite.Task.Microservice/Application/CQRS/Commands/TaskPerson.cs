using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.CQRS.Command
{
    public class TaskPerson
    {
        
        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public bool IsInternal { get; set; }

    }
}

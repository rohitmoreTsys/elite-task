using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Core;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository;

namespace Elite_Task.Microservice.Repository.Contracts
{
    public interface ITaskAttachmentRepository : IRepository<TaskAttachmentMapping>
    {
        TaskAttachmentMapping UpdateAttachment(TaskAttachmentMapping topicAttachmentMapping);

    }
}

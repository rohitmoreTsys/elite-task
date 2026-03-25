using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace Elite.Task.Microservice.Repository
{
    public class TaskAttachmentRepository : BaseDataAccess, ITaskAttachmentRepository
    {

        public TaskAttachmentRepository(EliteTaskContext context) : base(context)
        {
        }
        public TaskAttachmentMapping UpdateAttachment(TaskAttachmentMapping taskAttachmentMapping)
        {
            return _context.TaskAttachmentMapping.Update(taskAttachmentMapping).Entity;
        }       

    }
}

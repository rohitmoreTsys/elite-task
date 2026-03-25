using Elite.Task.Microservice.Application.CQRS.Commands.CommandsDto;
using Elite.Task.Microservice.Models.Entities;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Repository
{
    public class CommentRepository : BaseDataAccess, ICommentRepository
    {

        public CommentRepository(EliteTaskContext context) : base(context)
        {
        }

        public void Add(TaskComment comment)
        {
            _context.TaskComment.Add(comment);
        }

        public void Delete(List<string> GuiIDs)
        {
            foreach (var Id in GuiIDs)
            {
                var attachment = _context.TaskCommentAttachmentMapping.Where(a => a.AttachmentGuid == Id).FirstOrDefault();
                _context.TaskCommentAttachmentMapping.Remove(attachment);

            }
        }



        public void Update(TaskComment comment)
        {
            _context.Entry(comment).State = EntityState.Modified;
        }   
    }
}

using Elite_Task.Microservice.Models;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Repository
{
    public class TaskRepository : BaseDataAccess, ITaskRepository
    {
        public TaskRepository(EliteTaskContext context) : base(context)
        {
        }


        public void Add(EliteTask task)
        {
            _context.Add(task);

        }

        public void Update(EliteTask task)
        {
            _context.Entry(task).State = EntityState.Modified;

        }
        public async Task<List<EliteTask>> GetSubTaskListByIdAsync(long? id)
        {
            return await (from p in this.GetDBSet<EliteTask>()
                          where p.ParentId.Equals(id)
                          select p).ToListAsync();
        }
        public async Task<EliteTask> GetByIdAsync(long id)
        {
            return await (from p in this.GetDBSet<EliteTask>()
                          where p.Id.Equals(id)
                          select p).SingleOrDefaultAsync();
        }

       

        public async Task<EliteTask> GetTaskAndSubTaskByIdAsync(long id)
        {
            return await (from p in this.GetDBSet<EliteTask>()
                          .Include(EliteTask.PATH_EliteSubTask)
                          .Include(EliteTask.PATH_EliteTaskAttachment)
                          .Include(EliteTask.PATH_EliteTaskComment)
                          where p.Id.Equals(id)
                          select p).SingleOrDefaultAsync();
        }

        public async Task<List<EliteTask>> GetTaskAndSubTaskRangeByIdAsync(List<long> taskIds)
        {
            try
            {
                var deleteResult = await (from p in this.GetDBSet<EliteTask>()
                          .Include(EliteTask.PATH_EliteSubTask)
                          .Include(EliteTask.PATH_EliteTaskAttachment)
                          .Include(EliteTask.PATH_EliteTaskComment)
                                          where taskIds.Contains(p.Id)
                                          select p).ToListAsync();
                return deleteResult;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        public bool PutMeetingIdAsync(long _taskId)
        {
            try
            {
                if (_taskId > 0)
                {
                    var deleteMeeting = _context.EliteTask.Where(t => t.Id == _taskId).SingleOrDefault();
                    if (deleteMeeting != null)
                    {
                        deleteMeeting.MeetingId = null;
                        _context.Entry(deleteMeeting).State = EntityState.Modified;
                        _context.SaveChanges();
                        return true;
                    }
                }
                return false;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<IList<TaskComment>> GetComments(long taskId)
        {
            return await (from p in this.GetDBSet<TaskComment>()
                          where p.TaskId.Value.Equals(taskId)
                          select p)
                       .ToListAsync();
        }

        public int UpdateRange(IList<EliteTask> list)
        {
            _context.UpdateRange(list);
            return _context.SaveChanges();
        }

        public void DeleteAttachment(TaskAttachmentMapping attachment)
        {
            _context.Entry(attachment).State = EntityState.Deleted;
        }

        public void DeleteComments(TaskComment attachment)
        {
            _context.Entry(attachment).State = EntityState.Deleted;
        }

        public void DeleteTask(EliteTask task)
        {
            _context.Entry(task).State = EntityState.Deleted;
        }

        public async Task<long> GetCommitteeId(string attachmentid)
        {
            var taskid = _context.TaskAttachmentMapping.Where(g => g.AttachmentGuid == attachmentid).Select(x => x.TaskId).FirstOrDefaultAsync();
            if (taskid.Result > 0)
            {
                var committeeId = await _context.EliteTask.Where(x => x.Id == taskid.Result).Select(y => y.CommitteeId).FirstOrDefaultAsync();
                return committeeId.Value;
            }
            return 0;

        }

    }
}

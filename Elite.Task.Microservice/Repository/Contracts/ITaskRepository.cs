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
    public interface ITaskRepository : ICommonRepository<EliteTask>, IRepository<EliteTask>
    {

        Task<EliteTask> GetByIdAsync(long Id);

        Task<EliteTask> GetTaskAndSubTaskByIdAsync(long Id);
        Task <List<EliteTask>> GetSubTaskListByIdAsync(long? id);

        Task<List<EliteTask>> GetTaskAndSubTaskRangeByIdAsync(List<long> taskIds);

        Task<IList<TaskComment>> GetComments(long Id);

        int UpdateRange(IList<EliteTask> list);

        void DeleteAttachment(TaskAttachmentMapping attachment);

        void DeleteTask(EliteTask task);

        void DeleteComments(TaskComment attachment);
        bool PutMeetingIdAsync(long _taskId);

        //Task<NoticationEventStore> GetOldNotificationByIdAsync(long Id);

        Task<long> GetCommitteeId(string attachmentid);
    }
}

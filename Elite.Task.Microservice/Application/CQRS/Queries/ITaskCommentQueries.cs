using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries
{
    public interface ITaskCommentQueries
    {
        Task<IList<QueriesCommentDto>> GetCommentsByTaskID(long id);
        Task<List<string>> GetTaskAttachments(long taskCommentId);
    }
}

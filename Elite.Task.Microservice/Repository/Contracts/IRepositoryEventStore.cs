
using Elite.Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Repository.Contracts
{
    public interface IRepositoryEventStore : ICommonRepository<NoticationEventStore>, IRepository<NoticationEventStore>
    {
        Task<List<long>> GetByIds();

        Task<NoticationEventStore> GetById(long id);

        Task<List<long>> GetByTaskId(long taskId);
    }
}

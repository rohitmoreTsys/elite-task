
using Elite.EventBus.EventStore;
using Elite.EventBus.EventStoreEF;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Elite.EventBus.Services
{
    public interface IEventStoreService
    {
        Task<int> SaveEventAsync(List<EliteEventStoreDto> @event);
        Task<int> SaveEventAsync(EliteEventStoreDto @event, DbTransaction transaction);
        Task<int> MarkEventAsPublishedAsync(EliteEventStoreDto @event);
        Task<int> SaveEventAsync(List<EliteEventStoreDto> @event, DbTransaction transaction);
    }
}

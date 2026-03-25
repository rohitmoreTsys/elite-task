using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elite.EventBus.EventStore;
using Elite.EventBus.EventStoreEF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace Elite.EventBus.Services
{
    public class EventStoreService : IEventStoreService
    {
        private readonly NoticationEventStoreContext _eventStoreContext;
        private readonly DbConnection _dbConnection;

        public EventStoreService(DbConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException("dbConnection");
            _eventStoreContext = new NoticationEventStoreContext(
                new DbContextOptionsBuilder<NoticationEventStoreContext>()
                    .UseNpgsql(_dbConnection)
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);

        }



        public async Task<int> MarkEventAsPublishedAsync(EliteEventStoreDto @event)
        {
            var eventStore = await _eventStoreContext.NoticationEventStore.SingleAsync(ie => ie.Id == @event.Id);
            //need to be implement
            //update event store 

            _eventStoreContext.NoticationEventStore.Update(eventStore);

            return await _eventStoreContext.SaveChangesAsync();
        }






        public async Task<int> SaveEventAsync(EliteEventStoreDto @event, DbTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction", $"A {typeof(DbTransaction).FullName} is required as a pre-requisite to save the event.");
            }


            _eventStoreContext.Database.UseTransaction(transaction);
            _eventStoreContext.NoticationEventStore.Add(new NoticationEventStore()
            {
                Id = 1,
                ActionType = @event.ActionType,
                JsonMessage = @event.JsonMessage,
                CreatedDate = DateTime.Now,
                IsProcessed = false,
                GroupId = @event.GroupId,
                IsFailed = false,
                NotificationType = @event.NotificationType,
                Sourcetypeid = @event.Sourcetypeid
            });
            return await _eventStoreContext.SaveChangesAsync();
        }


        public async Task<int> SaveEventAsync(List<EliteEventStoreDto> @event, DbTransaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction", $"A {typeof(DbTransaction).FullName} is required as a pre-requisite to save the event.");
            }
            _eventStoreContext.Database.UseTransaction(transaction);
            _eventStoreContext.NoticationEventStore.AddRange(@event.Select(p => new NoticationEventStore()
            {

                ActionType = p.ActionType,
                JsonMessage = p.JsonMessage,
                CreatedDate = DateTime.Now,
                IsProcessed = false,
                GroupId = p.GroupId,
                IsFailed = false,
                NotificationType = p.NotificationType,
                Sourcetypeid = p.Sourcetypeid,
                IsReminder =p.IsReminder

            }).ToList<NoticationEventStore>());

            return await _eventStoreContext.SaveChangesAsync();
        }

        public async Task<int> SaveEventAsync(List<EliteEventStoreDto> @event)
        {
            _eventStoreContext.NoticationEventStore.AddRange(@event.Select(p => new NoticationEventStore()
            {

                ActionType = p.ActionType,
                JsonMessage = p.JsonMessage,
                CreatedDate = DateTime.Now,
                IsProcessed = false,
                GroupId = p.GroupId,
                IsFailed = false,
                NotificationType = p.NotificationType,
                Sourcetypeid = p.Sourcetypeid

            }).ToList<NoticationEventStore>());

            return await _eventStoreContext.SaveChangesAsync();
        }
    }
}

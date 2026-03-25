using Elite.Common.Utilities.NotificationServices;
using Elite.Task.Microservice.Repository.Contracts;
using Elite_Task.Microservice.Infrastructure.HostedServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Infrastructure.HostedServices
{
    public class TaskNotificationServices : HostedService
    {
        protected readonly IRepositoryEventStore _repositoryEventStore;
        protected readonly IMediator _mediator;
        public TaskNotificationServices(IRepositoryEventStore repositoryEventStore, IMediator mediator)
        {
            this._mediator = mediator;
            _repositoryEventStore = repositoryEventStore;
        }
        protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                await PublishTaskNotificationEvent();
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                continue;
            }
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private async System.Threading.Tasks.Task PublishTaskNotificationEvent()
        {
            var ids = await GetNotificationsIds();
            List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();

            if (ids?.Count > 0)
            {
                foreach (long id in ids)
                {
                    tasks.Add(System.Threading.Tasks.Task.Run(async () =>
                    {
                        await this._mediator.Send(new EmailNotificationCommand { Id = id });

                    }));
                }
            }

            System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
        }
        private async Task<List<long>> GetNotificationsIds()
        {
            return await _repositoryEventStore.GetByIds();
        }
    }
}

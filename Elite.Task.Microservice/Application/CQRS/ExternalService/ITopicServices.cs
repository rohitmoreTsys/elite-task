using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.ExternalService
{
    public interface ITopicServices
    {
        Task PublishTopicHistoryAsync(TopicHistoryEvent evt);
    }
}

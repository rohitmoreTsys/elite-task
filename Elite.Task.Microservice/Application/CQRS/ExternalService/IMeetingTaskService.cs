using Elite.Task.Microservice.Application.CQRS.IntegrationEvents.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elite.Common.Utilities.CommonType;


namespace Elite_Task.Microservice.Application.CQRS.ExternalService
{
    public interface IMeetingTaskService
    {
        Task PublishMeetingTaskThroughEventBusAsync(MeetingTaskEvent evt);

		Task PublishMeetingAgendaThroughEventBusAsync(MeetingAgendaEvent evt);

		Task PublishDeleteMeetingTaskThroughEventBusAsync(MeetingMinuteTaskDeleteEvent evt);

        Task<long> GetTopicId(long id);
        Task<string> GetAgendaById(long id);
		Task<bool> GetMeetingAsync(long id);
        Task<Elite.Common.Utilities.CommonType.MeetingInfo> GetMeetingInfo(long id,long taskid);
        Task<string> GetAgendaTitle(long id);
    }
}

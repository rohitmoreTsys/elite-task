using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.NotificationServices
{
    public interface IHTMLReadUtility
    {
        string TaskResponsible { get; }

        string TaskDelete { get; }
	    string TaskResponsibleChanged { get; }
        string TaskStatus { get; }
        string TaskCompleted { get; }
        string TaskRejected { get; }
        string TaskCoResponsible { get; }
		string TaskCoResponsibleChanged { get; }
        string MeetingTaskResponsible { get; }
        string MeetingTaskResponsibleBoM { get; }
        string MeetingTaskCoResponsible { get; }
        string MeetingTaskCoResponsibleBoM { get; }
        string TaskUpdate { get; }
    }
}

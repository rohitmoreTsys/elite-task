using Elite.Common.Utilities.JiraEntities;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
    public interface IJiraService
    {
        Task<JiraTicketResponse> CreateTaskInJira(string data);

        Task<string> UpdateTaskInJira(string jiraTicketData);

        Task<TaskInfoFromJira> GetTaskFromJira(string jiraTicketId);

        Task<string> SetTaskStatusToDeleteInJira(string jiraTicketKey);

        Task<bool> CheckJiraTokenValidity();
    }
}

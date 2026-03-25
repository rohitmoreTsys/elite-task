namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{
    public class QueriesJiraTicketInfoDto
    {
        public string JiraProjectName { get; set; }

        public string JiraProjectKey { get; set; }

        public string JiraIssueId { get; set; }

        public string JiraIssueKey { get; set; }
    }
}

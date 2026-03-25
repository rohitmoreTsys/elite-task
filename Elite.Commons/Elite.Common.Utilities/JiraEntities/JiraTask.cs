using Newtonsoft.Json;

namespace Elite.Common.Utilities.JiraEntities
{

    public class JiraTask
    {
        public JiraTask()
        {

        }
        public JiraTask(JiraTaskDto jiraTask)
        {
            this.fields = new Fields()
            {
                summary = jiraTask.Summary,
                description = string.IsNullOrEmpty(jiraTask.Description) ? string.Empty : jiraTask.Description,
                assignee = new Assignee() { name = JsonConvert.DeserializeObject<LookUpFields>(jiraTask.Assignee).Uid },
                reporter = new Reporter() { name = JsonConvert.DeserializeObject<LookUpFields>(jiraTask.Reporter).Uid },
                duedate = jiraTask.DueDate,
                issuetype = new Issuetype() { name = Constants.TASK_TYPE },
                priority = new Priority() { id = Constants.PRIORITY },
                project = new Project() { key = jiraTask.ProjectKey },
            };

        }
        public Fields fields { get; set; }
    }
}

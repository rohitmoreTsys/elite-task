using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{
    public class JiraTaskDto
    {

        public string IssueKey { get; set; }

        public string ProjectKey { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public string Reporter { get; set; }

        public string Assignee { get; set; }

        public string DueDate { get; set; }

        public string Status { get; set; }

        public IEnumerable<string> Attachments { get; set; }
    }
}

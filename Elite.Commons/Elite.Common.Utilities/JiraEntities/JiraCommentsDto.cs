using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{
    public class JiraCommentsDto
    {
        public string IssueKey { get; set; }
        public string body { get; set; }

        public IEnumerable<string> Attachments { get; set; }
    }
}

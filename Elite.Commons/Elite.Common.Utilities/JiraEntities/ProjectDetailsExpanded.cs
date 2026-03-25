using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{

    public class ProjectDetailsExpanded
    {
        public string expand { get; set; }
        public List<ProjectWithIssueType> projects { get; set; }
    }
}

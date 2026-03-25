using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{
    public class ProjectWithIssueType
    {
        public string self { get; set; }
        public string id { get; set; }
        public string key { get; set; }
        public string name { get; set; }
        public AvatarUrls avatarUrls { get; set; }
        public List<Issuetype> issuetypes { get; set; }
    }
}

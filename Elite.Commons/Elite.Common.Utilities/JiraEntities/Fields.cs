using Newtonsoft.Json;
using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{
    public class Fields
    {
        public Project project { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
        public Reporter reporter { get; set; }
        public Assignee assignee { get; set; }
        public Issuetype issuetype { get; set; }
        public Priority priority { get; set; }
        public string duedate { get; set; }

        [JsonIgnore]
        public List<JiraAttachment> attachment { get; set; }
    }
}

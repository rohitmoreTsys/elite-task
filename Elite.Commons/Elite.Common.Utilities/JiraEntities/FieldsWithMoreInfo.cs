using System.Collections.Generic;

namespace Elite.Common.Utilities.JiraEntities
{
    public class FieldsWithMoreInfo : Fields
    {

        public Status status { get; set; }
        public List<JiraAttachment> attachment { get; set; }
    }
}

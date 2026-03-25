using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{

    public class AttachmentPerson
    {
        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public List<LookUpFields> Users { get; set; }
    }

    public class LookUpFields
    {
        public LookUpFields()
        {

        }

        public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
    }

}

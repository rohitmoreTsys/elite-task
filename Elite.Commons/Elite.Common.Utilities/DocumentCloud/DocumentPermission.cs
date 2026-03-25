using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.DocumentCloud
{
    public class DocumentPermission
    {
        public long ID { get; set; }
        public long UserID { get; set; }
        public long CommitteeID { get; set; }
        public long RoleID { get; set; }
        public bool IsProcessed { get; set; }
        public bool IsSucess { get; set; }
        public bool IsRowInserted { get; set; }
        public bool isRowDeleted { get; set; }
        public string CommitteeName { get; set; }
        public string OwnCloudCommitteeID { get; set; }
    }
}

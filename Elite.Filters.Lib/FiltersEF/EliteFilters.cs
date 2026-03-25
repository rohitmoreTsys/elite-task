using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Filters.Lib.FiltersEF
{
    public partial class EliteFilters
    {
        public long Id { get; set; }
        public string FilterJson { get; set; }
        public string Uid { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public short FilterType { get; set; }
    }
}

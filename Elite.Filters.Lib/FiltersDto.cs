using Elite.Common.Utilities.CommonType;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Filters.Lib
{
    public class FiltersDto
    {   public long Id { get; set; }
        public string FilterJson { get; set; }
        public string Uid { get; set; }
        public FilterType FilterType { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
    }
}

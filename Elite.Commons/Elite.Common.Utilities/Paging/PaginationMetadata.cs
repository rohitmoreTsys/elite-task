using System;
using System.Collections.Generic;
using System.Text;

namespace Elite.Common.Utilities.Paging
{
    public class PaginationMetadata
    {
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }

    }
}

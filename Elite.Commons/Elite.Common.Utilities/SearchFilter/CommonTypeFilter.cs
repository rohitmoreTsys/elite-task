using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Elite.Common.Utilities.SearchFilter
{


    public class CommonTypeFilter<T> : IFilter<T> where T : class
    {
        public CommonTypeFilter()
        {
            Predicate = PredicateBuilder.False<T>();
        }

        public Expression<Func<T, bool>> Predicate { get; set; }

    }
}

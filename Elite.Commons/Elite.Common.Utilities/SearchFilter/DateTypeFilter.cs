
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.SearchFilter
{
    public class DateTypeFilter<T> : IFilter<T> where T : class
    {
        public DateTypeFilter()
        {
            Predicate = PredicateBuilder.False<T>();
        }

        public List<DateSearchOperator> SearchOprators { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }
        public void AddSearchCriteria(params DateSearchOperator[] operators)
        {

            SearchOprators = operators.ToList();


        }
    }
}

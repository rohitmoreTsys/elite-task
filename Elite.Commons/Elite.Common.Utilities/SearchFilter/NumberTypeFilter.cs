using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Elite.Common.Utilities.SearchFilter
{
    public class NumberTypeFilter<T> : IFilter<T> where T : class
    {
        public NumberTypeFilter()
        {
            Predicate = PredicateBuilder.False<T>();
        }

        public List<NumberSearchOperator> SearchOprators { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }
        public void AddSearchCriteria(params NumberSearchOperator[] operators)
        {
            SearchOprators = operators.ToList();
        }
    }
}

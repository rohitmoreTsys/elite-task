
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Elite.Common.Utilities.SearchFilter
{
    public class StringTypeFilter<T>: IFilter<T> where T: class
    {
        public StringTypeFilter()
        {
            Predicate = PredicateBuilder.False<T>();
        }

        public List<StringSearchOperator> SearchOprators { get; set; }

        public Expression<Func<T, bool>> Predicate { get; set; }
        public void AddSearchCriteria(params StringSearchOperator[] operators)
        {
            SearchOprators = operators.ToList();

        }
    }
}

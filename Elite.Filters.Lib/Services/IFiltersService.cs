using Elite.Common.Utilities.CommonType;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Filters.Lib.Services
{
    public interface IFilterQueries 
    {
        Task<dynamic> GetAsync(string uid, FilterType filterTyp);
    }

    public interface IFiltersService
    {
        Task<int> SaveAsync(FiltersDto @event);
        Task ClearFiltes(string uid, FilterType filterTyp);
    }
}

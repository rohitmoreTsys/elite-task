using Elite.Common.Utilities.CommonType;
using Elite.Filters.Lib.FiltersEF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Elite.Filters.Lib.Services
{
    public class FiltersService : IFiltersService, IFilterQueries
    {

        private readonly FiltereContext _context;
        private readonly DbConnection _dbConnection;

        public FiltersService(DbConnection dbConnection)
        {
            _dbConnection = dbConnection ?? throw new ArgumentNullException("dbConnection");
            _context = new FiltereContext(
                new DbContextOptionsBuilder<FiltereContext>()
                    .UseNpgsql(_dbConnection)
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options);
        }

        public async Task ClearFiltes(string uid, FilterType filterType)
        {
            var filter = await _context.EliteFilters
               .FirstOrDefaultAsync(p => p.Uid.ToUpper().Equals(uid.ToUpper()) && p.FilterType.Equals((short)filterType));
            if (filter != null)
            {
                _context.EliteFilters.Remove(filter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<dynamic> GetAsync(string uid, FilterType filterType)
        {
            var filter = await _context.EliteFilters.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Uid.ToUpper().Equals(uid.ToUpper()) && p.FilterType.Equals((short)filterType));

            if (filter != null)
            {
                return JsonConvert.DeserializeObject<dynamic>(filter.FilterJson);
            }
            return null;
        }

        public async Task<int> SaveAsync(FiltersDto @event)
        {
            var filter = await _context.EliteFilters
                              .FirstOrDefaultAsync(p => p.Uid.ToUpper().Equals(@event.Uid.ToUpper()) && p.FilterType.Equals((short)@event.FilterType));

            if (filter != null)
            {
                filter.Uid = @event.Uid;
                filter.FilterJson = @event.FilterJson;
                filter.FilterType = (short)(@event.FilterType);
                filter.IsActive = @event.IsActive;
                filter.ModifiedBy = @event.CreatedBy;
                filter.ModifiedDate = DateTime.Now;
                _context.EliteFilters.Update(filter);
            }
            else
            {
                var filterEntity = GetFilterEntity(@event, true);
                filterEntity.CreatedBy = @event.CreatedBy;
                filterEntity.CreatedDate = DateTime.Now;
                _context.EliteFilters.Add(filterEntity);
            }

            return await _context.SaveChangesAsync();
        }

        private EliteFilters GetFilterEntity(FiltersDto filtersDto, bool isUpdate = true)
        {
            return new EliteFilters()
            {
                Uid = filtersDto.Uid,
                FilterJson = filtersDto.FilterJson,
                FilterType = (short)(filtersDto.FilterType),
                IsActive = filtersDto.IsActive,
            };
        }
    }
}

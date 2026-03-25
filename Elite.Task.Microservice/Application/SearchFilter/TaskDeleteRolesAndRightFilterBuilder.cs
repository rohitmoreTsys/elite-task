using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.SearchFilter;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.SearchFilter
{
    public class TaskDeleteRolesAndRightFilterBuilder<T> : ITaskRolesAndRightFilterBuilder<T> where T : EliteTask, new()
    {
        private readonly List<IFilter<T>> _filters;
        private readonly IList<UserRolesAndRights> _userRolesRights;
        private readonly IRequestContext _requestContext;
        private readonly bool _isEliteClassic;

        #region Constructor
        public TaskDeleteRolesAndRightFilterBuilder(IList<UserRolesAndRights> userRolesRights, IRequestContext requestContext, bool isEliteClassic)
        {
            _filters = new List<IFilter<T>>();
            _userRolesRights = userRolesRights;
            _requestContext = requestContext;
            _isEliteClassic = isEliteClassic;
        }


        #endregion

        public Expression<Func<T, bool>> BuildFilter()
        {
            if (_userRolesRights?.Count > 0)
            {
                _userRolesRights.ToList().ForEach(p =>
                {
                    var action = p.Actions.Where(v => v.InternalKey.Equals((int)TaskEntityActionType.DeleteTask)).Single();
                    var filter = new CommonTypeFilter<T>();
                    if (action.IsAllowed)
                    {
                        if ((RolesType)p.RoleId == RolesType.CommitteeManager)
                        {
                            filter.Predicate = filter.Predicate.Or(c => (c.CreatedBy.Upper().Contains(p.UID.ToUpper())
                                                    && c.CommitteeId.Equals(p.CommitteeId))
                                                    && !c.MeetingId.HasValue);
                        }
                        else
                        {
                            if (_isEliteClassic)
                                filter.Predicate = filter.Predicate.Or(c => c.CreatedBy.Upper().Contains(p.UID.ToUpper()) && !c.MeetingId.HasValue);
                        }
                    }
                    _filters.Add(filter);
                });
            }

            var predicate = PredicateBuilder.False<T>();
            if (_filters?.Count > 0)
                _filters.ForEach(x => { predicate = predicate.Or(x.Predicate); });
            return predicate;
        }
    }
}

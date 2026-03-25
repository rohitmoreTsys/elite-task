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
    public class TaskUserRolesAndRightFilterBuilder<T> : ITaskRolesAndRightFilterBuilder<T> where T : EliteTask, new()
    {
        private readonly List<IFilter<T>> _filters;
        private readonly IList<UserRolesAndRights> _userRolesRights;
        private readonly IRequestContext _requestContext;
        private readonly bool _isDashboard;
        private readonly bool _filterWithResonsible;
        private readonly string assignUser = string.Empty;
        private readonly bool _isEliteClassic;


        #region Constructor
        public TaskUserRolesAndRightFilterBuilder(IList<UserRolesAndRights> userRolesRights, IRequestContext requestContext, bool isEliteClassic, bool isDashBoard = false, bool filterWithResonsible = false)
        {
            _filters = new List<IFilter<T>>();
            _userRolesRights = userRolesRights;
            _requestContext = requestContext;
            _isDashboard = isDashBoard;
            _filterWithResonsible = filterWithResonsible;
            assignUser = this._requestContext.IsDeputy ? this._requestContext.DeputyUID.Upper() : this._requestContext.UID.Upper();
            _isEliteClassic = isEliteClassic;
        }


        #endregion

        public Expression<Func<T, bool>> BuildFilter()
        {
            if (this._requestContext.IsDeputy)
                return this.GetFiltersForDeputy();
            else
                return this.GetFiltersForWithOutDeputy();

        }


        private Expression<Func<T, bool>> GetFiltersForWithOutDeputy()
        {
            if (_userRolesRights?.Count > 0)
            {
                _userRolesRights.ToList().ForEach(p =>
                {
                    var action = p.Actions.Where(v => v.InternalKey.Equals((int)TaskEntityActionType.ViewTask)).Single();
                    var filter = new CommonTypeFilter<T>();
                    if (_isEliteClassic && _isDashboard)
                    {
                        filter.Predicate = filter.Predicate.Or(c => c.Responsible.Upper().Contains("\"" + p.UID.ToUpper() + "\""))
                        .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains("\"" + p.UID.ToUpper() + "\"") : false);
                    }
                    else
                    {
                        filter.Predicate = filter.Predicate.Or(c => (c.CommitteeId == null && Convert.ToString(c.CreatedBy).ToUpper().Contains(p.UID.ToUpper())));

                        switch ((RolesType)p.RoleId)
                        {
                            case RolesType.User:
                                if (_isEliteClassic)
                                {
                                    filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CreatedBy != null ? Convert.ToString(c.CreatedBy).ToUpper().Contains(p.UID.ToUpper()) : false))
                                            && (c.CommitteeId != null && c.CommitteeId.HasValue && c.CommitteeId.Equals(p.CommitteeId)));
                                }
                                else
                                {
                                    filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false))
                                            && (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));
                                }
                                break;
                            case RolesType.CommitteeManager:
                                if (_isEliteClassic)
                                {
                                    filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CreatedBy != null ? Convert.ToString(c.CreatedBy).ToUpper().Contains(p.UID.ToUpper()) : false)));

                                    filter.Predicate = filter.Predicate
                                        .Or(c => (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                                }
                                else
                                    filter.Predicate = filter.Predicate.Or(c => (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                                break;

                            case RolesType.CoreMember:
                                if (_isEliteClassic)
                                {
                                    filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                            || (c.CreatedBy != null ? Convert.ToString(c.CreatedBy).ToUpper().Contains(p.UID.ToUpper()) : false)));

                                    filter.Predicate = filter.Predicate
                                        .Or(c => (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));
                                }
                                else
                                    filter.Predicate = filter.Predicate.Or(c => (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                                break;

                            case RolesType.CoreMemberExternal:
                                break;

                            case RolesType.Guest:
                                filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false))
                                        && (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                                break;

                            case RolesType.AssistantDocumentMember:
                                filter.Predicate = filter.Predicate.Or(c => c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false);

                                break;
                            case RolesType.Transient:
                                filter.Predicate = filter.Predicate.Or(c => c.CreatedBy.Upper().Contains(assignUser))
                                       .Or(c => c.Responsible.Upper().Contains(assignUser))
                                       .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false).Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                                       .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);

                                break;

                        }
                    }
                    _filters.Add(filter);
                });
            }
            else
            {
                var filter = new CommonTypeFilter<T>();

                if (_isDashboard)
                {
                    filter.Predicate = filter.Predicate.Or(c => c.Responsible.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false).Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);
                }
                else
                    filter.Predicate = filter.Predicate.Or(c => c.CreatedBy.Upper().Contains(assignUser))
                        .Or(c => c.Responsible.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false).Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);

                _filters.Add(filter);

            }

            var predicate = PredicateBuilder.False<T>();
            if (_filters?.Count > 0)
                _filters.ForEach(x => { predicate = predicate.Or(x.Predicate); });
            return predicate;
        }

        private Expression<Func<T, bool>> GetFiltersForDeputy()
        {
            if (_userRolesRights?.Count > 0)
            {
                _userRolesRights.ToList().ForEach(p =>
                {
                    var action = p.Actions.Where(v => v.InternalKey.Equals((int)TaskEntityActionType.ViewTask)).Single();
                    var filter = new CommonTypeFilter<T>();

                    switch ((RolesType)p.RoleId)
                    {
                        case RolesType.User:
                            if (_isEliteClassic)
                            {

                                filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                        || Convert.ToString(c.CreatedBy).ToUpper().Contains(p.UID.ToUpper()))
                                        && (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));
                            }
                            else
                            {
                                filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false))
                                    && (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));
                            }
                            break;
                        case RolesType.CommitteeManager:
                            filter.Predicate = filter.Predicate.Or(c => (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                            break;

                        case RolesType.CoreMember:
                            filter.Predicate = filter.Predicate.Or(c => c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false);

                            break;
                        case RolesType.CoreMemberExternal:
                            break;

                        case RolesType.Guest:
                            filter.Predicate = filter.Predicate.Or(c => ((c.Responsible != null ? c.Responsible.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.CoResponsibles != null ? c.CoResponsibles.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.ResponsibleEmailRecipient != null ? c.ResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false)
                                    || (c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(p.UID.ToUpper()) : false))
                                    && (c.CommitteeId != null ? (c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false) : false));

                            break;

                        case RolesType.AssistantDocumentMember:
                            filter.Predicate = filter.Predicate.Or(c => c.CommitteeId.HasValue ? c.CommitteeId.Equals(p.CommitteeId) : false);

                            break;
                        case RolesType.Transient:
                            filter.Predicate = filter.Predicate.Or(c => c.CreatedBy.Upper().Contains(assignUser))
                                   .Or(c => c.Responsible.Upper().Contains(assignUser))
                                   .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false).Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                                   .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);

                            break;

                    }

                    _filters.Add(filter);
                });
            }
            else
            {
                var filter = new CommonTypeFilter<T>();

                if (_isDashboard)
                {
                    filter.Predicate = filter.Predicate.Or(c => c.Responsible.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false)
                        .Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);
                }
                else
                    filter.Predicate = filter.Predicate.Or(c => c.CreatedBy.Upper().Contains(assignUser))
                        .Or(c => c.Responsible.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibles != null ? c.CoResponsibles.Upper().Contains(assignUser) : false)
                        .Or(c => c.ResponsibleEmailRecipient.Upper().Contains(assignUser))
                        .Or(c => c.CoResponsibleEmailRecipient != null ? c.CoResponsibleEmailRecipient.Upper().Contains(assignUser) : false);

                _filters.Add(filter);

            }

            var predicate = PredicateBuilder.False<T>();
            if (_filters?.Count > 0)
                _filters.ForEach(x => { predicate = predicate.Or(x.Predicate); });
            return predicate;
        }

    }
}

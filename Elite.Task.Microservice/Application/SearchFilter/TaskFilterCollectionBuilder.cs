using Elite.Common.Utilities.Extensions;
using Elite.Common.Utilities.SearchFilter;
using Elite_Task.Microservice.CommonLib;
using Elite_Task.Microservice.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Elite_Task.Microservice.Application.SearchFilter
{
    public class TaskFilterCollectionBuilder<T> where T : EliteTask, new()
    {
        #region Variable Declaration

        private readonly List<IFilter<T>> _filters;
        private readonly TaskSearchKeywords _keywords;
        private IList<IOrderByExpression<T>> _orderByExpression;


        #endregion

        #region Constructor
        public TaskFilterCollectionBuilder(TaskSearchKeywords keywords)
        {
            _filters = new List<IFilter<T>>();
            _keywords = keywords;
            _orderByExpression = new List<IOrderByExpression<T>>();

        }
        #endregion

        #region  Public Functions
        public IQueryable<T> ApplyOrderBy(IQueryable<T> query)
        {
            if (_orderByExpression?.Count > 0)
                return OrderBy.ApplyOrderBy(query, _orderByExpression.ToArray());
            else
                return OrderBy.ApplyOrderBy(query, new OrderByExpression<T, int?>(p => p.Status.Value, false), new OrderByExpression<T, DateTime?>(p => p.DueDate.HasValue ? p.DueDate.Value : DateTime.MaxValue, false));

        }

        //public List<IFilter<T>> BuildFilter()
        public Expression<Func<T, bool>> BuildFilter()
        {
            #region task Title
            if (!string.IsNullOrWhiteSpace(_keywords.TaskTitle))
            {
                var taskTitleFilter = new StringTypeFilter<T>();

                taskTitleFilter.AddSearchCriteria(StringSearchOperator.Contains);

                taskTitleFilter.SearchOprators.ForEach(op =>
                {
                    if (op == StringSearchOperator.Contains)
                    {
                        taskTitleFilter.Predicate = taskTitleFilter.Predicate.Or(p => p.Title.ToUpper().Contains(_keywords.TaskTitle.ToUpper(), StringComparison.InvariantCultureIgnoreCase));

                        //taskTitleFilter.Predicate = taskTitleFilter.Predicate.And(p => p.Parent != null ?  p.Parent.Title.ToUpper().Contains(_keywords.TaskTitle.ToUpper()) : false);
                    }
                    if (op == StringSearchOperator.Equals)
                    {
                        taskTitleFilter.Predicate = taskTitleFilter.Predicate.Or(p => p.Title.ToUpper().Equals(_keywords.TaskTitle.ToUpper()));
                    }

                    if (op == StringSearchOperator.StartsWith)
                    {
                        taskTitleFilter.Predicate = taskTitleFilter.Predicate.Or(p => p.Title.ToUpper().StartsWith(_keywords.TaskTitle.ToUpper()));

                        //taskTitleFilter.Predicate = taskTitleFilter.Predicate.Or(p => p.Parent != null ? p.Parent.Title.ToUpper().StartsWith(_keywords.TaskTitle.ToUpper()) : false);
                    }

                    if (op == StringSearchOperator.EndsWith)
                    {
                        taskTitleFilter.Predicate = taskTitleFilter.Predicate.Or(p => p.Title.ToUpper().EndsWith(_keywords.TaskTitle.ToUpper()));
                    }

                });

                // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.Title));

                _filters.Add(taskTitleFilter);
            }
            #endregion


            #region  task responsible
            if (_keywords.Responsible?.Count > 0 || _keywords.CoResponsibles?.Count > 0)
            {
                var responsibleFilter = new StringTypeFilter<T>();
                if (_keywords.CoResponsibles?.Count > 0)
                {
                    foreach (var resp in _keywords.CoResponsibles)
                        _keywords.Responsible.Add(resp);
                }

                _keywords.Responsible.ForEach(x =>
                {
                    responsibleFilter.AddSearchCriteria(StringSearchOperator.Contains);
                    responsibleFilter.SearchOprators.ForEach(op =>
                    {

                        if (op == StringSearchOperator.Contains)
                        {
                            responsibleFilter.Predicate = responsibleFilter.Predicate.Or(p => p.Responsible.ToString().ToUpper().Contains(x.ToUpper()))
                            .Or(p => p.ResponsibleEmailRecipient != null ? p.ResponsibleEmailRecipient.ToString().ToUpper().Contains(x.ToUpper()): false)
                            .Or(p => p.CoResponsibleEmailRecipient != null ? p.CoResponsibleEmailRecipient.ToString().ToUpper().Contains(x.ToUpper()): false)
                            .Or(p => p.CoResponsibles != null ? p.CoResponsibles.ToString().ToUpper().Contains(x.ToUpper()) : false);
                        }
                        if (op == StringSearchOperator.Equals)
                        {
                            responsibleFilter.Predicate = responsibleFilter.Predicate.Or(p => p.Responsible.ToString().ToUpper().Equals(x.ToUpper()))
                            .Or(p => p.ResponsibleEmailRecipient != null ? p.ResponsibleEmailRecipient.ToString().ToUpper().Equals(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibleEmailRecipient != null ? p.CoResponsibleEmailRecipient.ToString().ToUpper().Equals(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibles != null ? p.CoResponsibles.ToString().ToUpper().Equals(x.ToUpper()) : false);
                        }

                        if (op == StringSearchOperator.StartsWith)
                        {
                            responsibleFilter.Predicate = responsibleFilter.Predicate.Or(p => p.Responsible.ToString().ToUpper().StartsWith(x.ToUpper()))
                            .Or(p => p.ResponsibleEmailRecipient != null ? p.ResponsibleEmailRecipient.ToString().ToUpper().StartsWith(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibleEmailRecipient != null ? p.CoResponsibleEmailRecipient.ToString().ToUpper().StartsWith(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibles != null ? p.CoResponsibles.ToString().ToUpper().StartsWith(x.ToUpper()) : false);
                        }

                        if (op == StringSearchOperator.EndsWith)
                        {
                            responsibleFilter.Predicate = responsibleFilter.Predicate.Or(p => p.Responsible.ToString().ToUpper().EndsWith(x.ToUpper()))
                            .Or(p => p.ResponsibleEmailRecipient != null ? p.ResponsibleEmailRecipient.ToString().ToUpper().EndsWith(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibleEmailRecipient != null ? p.CoResponsibleEmailRecipient.ToString().ToUpper().EndsWith(x.ToUpper()) : false)
                            .Or(p => p.CoResponsibles != null ? p.CoResponsibles.ToString().ToUpper().EndsWith(x.ToUpper()) : false);
                        }



                    });
                    // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.ResponsibleJson));
                    _filters.Add(responsibleFilter);
                });
            }
            #endregion
            #region  task responsible Division
            if (_keywords.ResponsibleDivision?.Count > 0)
            {
                var responsibleDivFilter = new StringTypeFilter<T>();


                _keywords.ResponsibleDivision.ForEach(x =>
                {
                    responsibleDivFilter.AddSearchCriteria(StringSearchOperator.Contains);
                    responsibleDivFilter.SearchOprators.ForEach(op =>
                    {

                        if (op == StringSearchOperator.Contains)
                        {
                            responsibleDivFilter.Predicate = responsibleDivFilter.Predicate.Or(p => p.ResponsibleDivision.ToString().ToUpper().Contains(x.ToUpper()));
                        }
                        if (op == StringSearchOperator.Equals)
                        {
                            responsibleDivFilter.Predicate = responsibleDivFilter.Predicate.Or(p => p.ResponsibleDivision.ToString().ToUpper().Equals(x.ToUpper()));
                        }

                        if (op == StringSearchOperator.StartsWith)
                        {
                            responsibleDivFilter.Predicate = responsibleDivFilter.Predicate.Or(p => p.ResponsibleDivision.ToString().ToUpper().StartsWith(x.ToUpper()));
                        }

                        if (op == StringSearchOperator.EndsWith)
                        {
                            responsibleDivFilter.Predicate = responsibleDivFilter.Predicate.Or(p => p.ResponsibleDivision.ToString().ToUpper().EndsWith(x.ToUpper()));
                        }
                    });
                    // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.ResponsibleJson));
                    _filters.Add(responsibleDivFilter);
                });
            }
            #endregion
            #region Task committee
            if (_keywords.CommitteeId?.Count > 0)
            {
                var committeeFilter = new NumberTypeFilter<T>();
                _keywords.CommitteeId.ForEach(x =>
                {
                    if (x.HasValue)
                    {
                        committeeFilter.AddSearchCriteria(NumberSearchOperator.EqualsTo);
                        committeeFilter.SearchOprators.ForEach(op =>
                        {
                            if (op == NumberSearchOperator.EqualsTo)
                            {
                                committeeFilter.Predicate = committeeFilter.Predicate.Or(p => p.CommitteeId.HasValue ? p.CommitteeId.Value == x : false);
                            }
                        });

                        //_orderByExpression.Add(new OrderByExpression<T, long>(u => u.TargetCommitteeId.Value));
                        _filters.Add(committeeFilter);
                    }
                });
            }
            #endregion

            #region Task Id
            if (_keywords.Id.HasValue)
            {
                var idFilter = new NumberTypeFilter<T>();
                idFilter.AddSearchCriteria(NumberSearchOperator.EqualsTo);
                idFilter.SearchOprators.ForEach(op =>
                {
                    if (op == NumberSearchOperator.EqualsTo)
                    {
                        idFilter.Predicate = idFilter.Predicate.Or(p => p.Id == _keywords.Id.Value ? true : false);
                    }
                });
                _filters.Add(idFilter);
            }
            #endregion

            #region task due data
            if (_keywords.DueStartDate.HasValue && _keywords.DueEndDate.HasValue)
            {
                var dueDateFilter = new DateTypeFilter<T>();
                dueDateFilter.AddSearchCriteria(DateSearchOperator.Between);
                dueDateFilter.SearchOprators.ForEach(op =>
                {

                    if (op == DateSearchOperator.Between)
                    {
                        dueDateFilter.Predicate = dueDateFilter.Predicate.Or(p => (p.DueDate.HasValue) ? p.DueDate.Value >= _keywords.DueStartDate.Value && p.DueDate.Value <= _keywords.DueEndDate.Value : false);
                    }

                    if (op == DateSearchOperator.CurrentDate)
                    {
                        dueDateFilter.Predicate = dueDateFilter.Predicate.Or(p => p.DueDate.Value >= _keywords.DueStartDate.Value && p.DueDate.Value <= DateTime.Now);
                    }

                });
                //_orderByExpression.Add(new OrderByExpression<T, DateTime>(u => u.DueDate.Value));
                _filters.Add(dueDateFilter);
            }
            #endregion

            #region task status --> Assigned, InProgress and Completed
            var taskFilter = new StringTypeFilter<T>();
            if (_keywords.TaskType != null)
                if (_keywords.TaskType[0] || _keywords.TaskType[1] || _keywords.TaskType[2])
                {
                    //Status == IsAssignee              
                    if (_keywords.TaskType[0])
                    {
                        taskFilter.AddSearchCriteria(StringSearchOperator.Equals);
                        taskFilter.SearchOprators.ForEach(op =>
                        {
                            if (op == StringSearchOperator.Equals)
                            {
                                taskFilter.Predicate = taskFilter.Predicate.Or(p => p.Status.Equals((int)TaskStatus.Assigned));
                            }
                        });
                        // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.Status));
                    }

                    //Status == InProgress
                    if (_keywords.TaskType[1])
                    {
                        taskFilter.AddSearchCriteria(StringSearchOperator.Equals);
                        taskFilter.SearchOprators.ForEach(op =>
                        {
                            if (op == StringSearchOperator.Equals)
                            {
                                taskFilter.Predicate = taskFilter.Predicate.Or(p => p.Status.Equals((int)TaskStatus.InProgress));
                            }
                        });
                        // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.Status));
                    }

                    //Status == Compeleted
                    if (_keywords.TaskType[2])
                    {
                        taskFilter.AddSearchCriteria(StringSearchOperator.Equals);
                        taskFilter.SearchOprators.ForEach(op =>
                        {
                            if (op == StringSearchOperator.Equals)
                            {
                                taskFilter.Predicate = taskFilter.Predicate.Or(p => p.Status.Equals((int)TaskStatus.Completed));
                            }
                        });

                        // _orderByExpression.Add(new OrderByExpression<T, string>(u => u.Status));
                    }

                    _filters.Add(taskFilter);
                }
            //else
            //    taskFilter.Predicate = taskFilter.Predicate.Or(p => false);



            #endregion

            #region  Adding Filter options
            var predicate = PredicateBuilder.True<T>();
            //applied filter for main task only
            // predicate = predicate.And<T>(p => p.ParentId.HasValue);            

            if (_filters?.Count > 0)
                _filters.ForEach(x => { predicate = predicate.And(x.Predicate); });

            #endregion

            return predicate;
        }
        #endregion
    }
}

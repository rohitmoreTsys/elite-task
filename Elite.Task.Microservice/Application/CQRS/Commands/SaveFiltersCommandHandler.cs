using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Filters.Lib;
using Elite.Filters.Lib.Services;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite_Task.Microservice.Models.Entities;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class SaveFiltersCommandHandler : IRequestHandler<SaveFiltersCommand, bool>
    {
        private readonly Func<DbConnection, IFiltersService> _filtersServiceFactory;
        private readonly IRequestContext _requestContext;
        private readonly IFiltersService _filtersService;

        private readonly ITaskLog _taskLog;
        private readonly string _UID;

        public SaveFiltersCommandHandler(ITaskRepository repository, IRequestContext requestContext, Func<DbConnection, IFiltersService> filtersServiceFactory, ITaskLog taskLog)
        {
            _requestContext = requestContext;
            _filtersServiceFactory = filtersServiceFactory;
            _filtersService = this._filtersServiceFactory(((EliteTaskContext)repository.UnitOfWork).Database.GetDbConnection());
            _taskLog = taskLog;
            this._UID = this._requestContext.IsDeputy ? this._requestContext.DeputyUID.Upper() : _requestContext.UID.ToUpper();
        }

        public async Task<bool> Handle(SaveFiltersCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.ClearFilters)
                {
                    await _filtersService.ClearFiltes(this._requestContext.UID, FilterType.Task);
                }
                else
                {

                    FiltersDto filtersDto = new FiltersDto();
                    filtersDto.FilterJson = request.FilterJson;
                    filtersDto.Uid = this._UID;
                    filtersDto.CreatedBy = this._UID;
                    filtersDto.IsActive = true;
                    filtersDto.FilterType = FilterType.Task;
                    await _filtersService.SaveAsync(filtersDto);
                }
            }
            catch (Exception)
            {
				throw;
            }

            return true;
        }
    }
}

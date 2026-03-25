using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elite_Task.Microservice.CommonLib;
using Elite_Task.Microservice.Application.Paging;
using Elite_Task.Microservice.Application.SearchFilter;
using Elite.Common.Utilities.Paging;
using Microsoft.AspNetCore.Http;
using Elite.Task.Microservice.Application.CQRS.Queries;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Common.Utilities.CommonType;
using Microsoft.AspNetCore.Mvc;
using Elite.Common.Utilities.DocumentCloud;
using Elite.Task.Microservice.Models.Entities;

namespace Elite_Task.Microservice.Application.CQRS.Queries
{
   public interface ITaskQueries
    {

        Task<QueriesTaskDto> GetTaskAsync(long id);

        Task<IList<QueriesTaskListDto>> GetTasks(TaskSearchKeywords searchKeywords);
        Task<List<AttachmentMappingDetails>> GetTaskAttachments( );

        Task<IList<QueriesTaskListDto>> GetTaskByPagination(int? page, int listType, TaskSearchKeywords topicSearchKeywords, HttpResponse response, FilterActionEnum topicFilterAction);

        Task<IList<QueriesDeshboardTask>> GetTaskForDashboard();

        Task<IList<QueriesPDFTaskDto>> GetPDFTask(long [] ids);

        Task<IList<LookUp>> GetAllUserCommitteesForTask(string uid);

        Task<dynamic> GetFilters();
        Task<IActionResult> GetTasksForEXCEL(int? page,TaskSearchKeywords searchKeywords, FilterActionEnum topicFilterAction, HttpResponse response);
        Task<List<QueriesTaskListDto>> GetTasksforDownload(int? page,TaskSearchKeywords searchKeywords, FilterActionEnum topicFilterAction);
        Task<IActionResult> GetPdfData(int? page,TaskSearchKeywords taskSearchKeywords, FilterActionEnum taskFilterAction, HttpResponse response);
        Task<dynamic> GetTaskDescription(long taskID);
        Task<IList<TaskOverdueSummary>> GetTaskOverdueSummaryAsync(string committees, string divisions, string startDate, string endDate);
        Task<IList<Division>> GetDivisionAsync(long[] committeeids);
        Task<IList<string>> GetDepartment(string dep);
        Task<IList<CommitteeTaskSeries>> GetTaskLineChartDataAsync(string committees, string divisions, string startDate, string endDate);
        Task<IList<LookUp>> GetListCMUserCommittees(string uid);
        Task<IList<GlobalSearchTaskDto>> GetAllTasksContentPaginated(int? page, int listType, string searchKeywords, HttpResponse response, FilterActionEnum taskFilterAction, string uid);

    }
}

using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Common.Utilities.ResponseHandling;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Task.Microservice.NotificationServices;
using Elite.Topic.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Queries;
using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite_Task.Microservice.Application.SearchFilter;
using Elite_Task.Microservice.Repository.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Elite_Task.Microservice.Controllers
{
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        #region private Variable
        private readonly IMediator _mediator;
        private readonly ITaskQueries _taskQueries;
        private readonly ITaskLog _taskLog;
        private readonly ITaskRepository _taskRepo;
        private readonly IJiraService _jiraService;
        private readonly Func<IConfiguration, IRequestContext, IJiraService> _jiraServiceFactory;
        #endregion Variable

        #region Constructor
        public TaskController(IMediator mediator, ITaskQueries taskQueries, ITaskLog taskLog,
            ITaskRepository taskRepository, IConfiguration configuration, IRequestContext requestContext,
            Func<IConfiguration, IRequestContext, IJiraService> jiraServiceFactory)
        {
            _mediator = mediator;
            _taskQueries = taskQueries;
            _taskLog = taskLog;
            _taskRepo = taskRepository;
            _jiraServiceFactory = jiraServiceFactory;
            _jiraService = _jiraServiceFactory(configuration, requestContext);
        }

        #endregion


        #region Get Tasks with Paging
        // GET api/values
        [HttpGet]
        [Route("page/{page}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskWithPaginate(int? page, [FromQuery] FilterActionEnum taskFilterAction, [FromQuery] int listType)
        {
            try
            {
                var headers = this.Request.Headers;
                if (headers.ContainsKey(nameof(TaskSearchKeywords)))
                {
                    StringValues value = default(StringValues);
                    if (headers.TryGetValue(nameof(TaskSearchKeywords), out value))
                    {
                        var taskSearchKeywords = JsonConvert.DeserializeObject<TaskSearchKeywords>(value);
                        var tasks = await _taskQueries.GetTaskByPagination(page, listType, taskSearchKeywords, Response, taskFilterAction);

                        if (tasks?.Count > 0)
                            return Ok(tasks);
                    }
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        [HttpPost]
        [Route("FilterTaskSearch/{page}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskFilterSearchWithPaginate([FromQuery] int? page, [FromQuery] FilterActionEnum taskFilterAction, [FromQuery] int listType,
            [FromBody] TaskSearchKeywords taskSearch)
            {

            try
            {
                if (taskSearch != null)
                {
                    if (taskSearch.savefilter)
                    {
                        var commandResult = await _mediator.Send(new SaveFiltersCommand
                        {
                            FilterJson = JsonConvert.SerializeObject(taskSearch),
                            ClearFilters = false
                        });

                    }
                    if (taskSearch.isClearSearch)
                    {
                        var commandResult = await _mediator.Send(new SaveFiltersCommand
                        {
                            ClearFilters = true
                        });
                    }

                    var taskSearchKeywords = await _taskQueries.GetTaskByPagination(page, listType, taskSearch, Response, taskFilterAction);
                    if (taskSearchKeywords?.Count > 0)
                        return Ok(taskSearchKeywords);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }


        // GET api/values
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTasks()
        {
            try
            {
                var headers = this.Request.Headers;
                if (headers.ContainsKey(nameof(TaskSearchKeywords)))
                {
                    StringValues value = default(StringValues);
                    if (headers.TryGetValue(nameof(TaskSearchKeywords), out value))
                    {
                        var taskSearchKeywords = JsonConvert.DeserializeObject<TaskSearchKeywords>(value);
                        var tasks = await _taskQueries.GetTasks(taskSearchKeywords);

                        if (tasks?.Count > 0)
                            return Ok(tasks);
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }

        #endregion

        #region Get Task 
        // GET api/values
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTask(long id)
        {
            try
            {
                var task = await _taskQueries.GetTaskAsync(id);
                if (task != null && task.Id > 0)
                {
                    return Ok(task);
                }

                return NotFound();
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                if (ex.Message.ToLower() == "unauthorized")
                {
                    return NotFound();
                }
                return BadRequest();

            }
        }

        // GET api/values
        [HttpGet("jira/{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SyncJiraTask(long id)
        {
            try
            {
                var cmd = new JiraTaskCommand();
                cmd.Id = id;
                var result = await _mediator.Send(cmd);
                var updatedTask = await _taskQueries.GetTaskAsync(id);
                return Ok(updatedTask);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
        }

        #endregion

        #region Get Task for Dashboard
        [HttpGet]
        [Route("GetTaskForDashboard")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskForDashboard()
        {
            try
            {
                var topic = await _taskQueries.GetTaskForDashboard();
                if (topic != null)
                {
                    return Ok(topic);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Get Task For PDF
        [HttpGet]
        [Route("GetTaskForPDF")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskForPDF([FromQuery] long[] ids)
        {
            try
            {
                var topic = await _taskQueries.GetPDFTask(ids);
                if (topic != null)
                {
                    return Ok(topic);
                }

                return NotFound();
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
        }
        #endregion

        #region Task and Subtask save
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public async Task<IActionResult> Post([FromBody] TaskCommand command)
        {
            try
            {
                if (command is null)
                    return BadRequest();

                //Stop a deputy from creating or updating a jira task 
                if (command.JiraTicketInfo != null)
                {
                    var headers = this.Request.Headers;
                    if (headers.ContainsKey("uid") && headers.ContainsKey("deputyid"))
                    {
                        return Forbid();
                    }
                }


                var commandResult = await _mediator.Send(command);
                return commandResult > 0 ? (IActionResult)Ok(commandResult) : (IActionResult)BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                if (ex.Message.ToLower() == "unauthorized")
                {
                    return NotFound();
                }
                return BadRequest();
            }

        }
        #endregion

        #region Delete
        // DELETE api/values/5
        [HttpPost]
        [Route("DeleteTask")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteTask([FromBody] MeetingTaskDeleteCommand command)
        {
            bool commandResult = false;
            try
            {
                if (command is null)
                    return BadRequest();
                commandResult = await _mediator.Send(command);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                if (ex.Message.ToLower() == "unauthorized")
                {
                    return NotFound();
                }
                return BadRequest();
            }
            return commandResult ? (IActionResult)Ok(commandResult) : (IActionResult)BadRequest();

        }
        #endregion

        #region Transient User Display Committees
        [HttpGet]
        [Route("GetAllUserCommittees")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAllUserCommittees()
        {
            try
            {
                var headers = this.Request.Headers;
                if (headers.ContainsKey("uid"))
                {
                    StringValues value = default(StringValues);
                    if (headers.TryGetValue("uid", out value))
                    {
                        var uid = value;
                        var committees = await _taskQueries.GetAllUserCommitteesForTask(uid);
                        return Ok(committees);
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion


        #region Meeting Published and UnPublished
        [HttpPost("MeetingPublishedandUnPublished")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> MeetingPublishedandUnPublished([FromBody] TaskPublishedAndUnPublishedCommand Command)
        {
            try
            {
                if (Command is null)
                    return BadRequest();

                var commandResult = await _mediator.Send(Command);
                return commandResult ? (IActionResult)Ok() : (IActionResult)BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }

        }
        #endregion

        #region Excel download
        [HttpPost("taskEXCELDownload/{pageIndex}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> taskEXCELDownload(int? pageIndex, [FromQuery] FilterActionEnum topicFilterAction, [FromBody] TaskSearchKeywords topicSearch)
        {
            try
            {
                if (topicSearch != null)
                {
                    var topicSearchKeywords = await _taskQueries.GetTasksForEXCEL(pageIndex, topicSearch, topicFilterAction, Response);
                    return topicSearchKeywords;

                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }

        }
        #endregion

        #region PDF Download for Task List
        [HttpPost("taskPDFDownload/{pageIndex}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> taskPDFDownload(int? pageIndex, [FromQuery] FilterActionEnum taskFilterAction, [FromBody] TaskSearchKeywords taskSearchKeywords)
        {
            try
            {
                if (taskSearchKeywords != null)
                {
                    var tasks = await _taskQueries.GetPdfData(pageIndex, taskSearchKeywords, taskFilterAction, Response);
                    return tasks;
                }
                return NotFound();

            }
            catch (Exception ex)
            {
                return BadRequest();
            }


        }
        #endregion

        #region Get Filters
        [HttpGet("GetFilters")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetFilters()
        {
            try
            {
                var topic = await _taskQueries.GetFilters();

                if (topic != null)
                {
                    return Ok(topic);
                }

                return Ok(null);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion
        #region Get TaskDescription       
        [HttpGet]
        [Route("TaskDescription/{taskId}")]
        public async Task<IActionResult> TaskDescription(long taskId)
        {
            try
            {
                var taskDesc = await _taskQueries.GetTaskDescription(taskId);

                if (taskDesc != null)
                {
                    return (IActionResult)Ok(taskDesc);
                }

                return Ok(null);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion
        #region Meeting Published and UnPublished
        [HttpPost("UpdateTopicMeetingDetails")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateTopicMeetingDetails([FromBody] UpdateTopicMeetingDetailsCommand Command)
        {
            try
            {
                if (Command is null)
                    return BadRequest();

                var commandResult = await _mediator.Send(Command);
                return commandResult ? (IActionResult)Ok() : (IActionResult)BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }

        }
        #endregion

        #region Remove of Meeting Id
        [HttpPut("RemoveMeetingId")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> RemoveMeetingId([FromBody] long id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var commandResult = _taskRepo.PutMeetingIdAsync(id);
                return commandResult ? (IActionResult)Ok() : (IActionResult)BadRequest();



            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }

        }
        #endregion


        #region "Get MeetingID from attachment"

        [HttpGet("GetCommitteeIdfromAttachmentId/{attachmentid}")]
        [ActionName("GetCommitteeIdfromAttachmentId/{attachmentid}")]
        public async Task<long> GetCommitteeIdfromAttachmentId(string attachmentid)
        {

            var comid = await _taskRepo.GetCommitteeId(attachmentid);
            return comid;
        }

        #endregion


        #region "Get 2 types of email Templates for meeting task"
        /// <summary>
        /// Get 2 types of email Templates for meeting task
        /// </summary>
        /// <returns> Email Templates</returns>
        [HttpGet("GetEmailTemplate")]
        [ActionName("GetEmailTemplate")]
        public async Task<IActionResult> GetEmailTemplate()
        {
            try
            {
               EmailTemplatesQuery query = new EmailTemplatesQuery();
                var emailTemplate = await _mediator.Send(query);
                return (IActionResult)Ok(emailTemplate);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion

        #region Get Task Overdue data
        [HttpGet]
        [Route("GetTaskOverdueSummary")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskOverdueSummary([FromQuery] string committees, [FromQuery] string divisions, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                var result = await _taskQueries.GetTaskOverdueSummaryAsync(committees, divisions, startDate, endDate);
                return Ok(result);

                //var headers = this.Request.Headers;
                //if (headers.ContainsKey("uid"))
                //{
                //    StringValues value = default(StringValues);
                //    if (headers.TryGetValue("uid", out value))
                //    {
                //        var uid = value;
                //        var result = await _taskQueries.GetTaskOverdueSummaryAsync();
                //        return Ok(result);

                //    }
                //}
                //return NotFound();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion
        #region Get Division
        [HttpGet]
        [Route("GetDivision")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDivision([FromQuery] long[] committees)
        {
            try
            {
                var result = await _taskQueries.GetDivisionAsync(committees);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion

        #region Get Department
        [HttpGet("GetDepartment")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetDepartment(string department)
        {
            try
            {
                var topic = await _taskQueries.GetDepartment(department);

                if (topic != null)
                {
                    return Ok(topic);
                }

                return Ok(null);
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion

        #region Get Task Line Chart Data Async

        [HttpGet]
        [Route("GetTaskLineChartDataAsync")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetTaskLineChartDataAsync([FromQuery] string committees, [FromQuery] string divisions, [FromQuery] string startDate, [FromQuery] string endDate)
        {
            try
            {
                var result = await _taskQueries.GetTaskLineChartDataAsync(committees, divisions, startDate, endDate);
                return Ok(result);

            }
            catch (Exception exa)
            {

                _taskLog.LogTaskError(exa);
                return BadRequest();
            }
        }
        #endregion

        #region Transient User Display Committees
        [HttpGet]
        [Route("GetListCMUserCommittees")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetListCMUserCommittees()
        {
            try
            {
                var headers = this.Request.Headers;
                if (headers.ContainsKey("uid"))
                {
                    StringValues value = default(StringValues);
                    if (headers.TryGetValue("uid", out value))
                    {
                        var uid = value;
                        var committees = await _taskQueries.GetListCMUserCommittees(uid);
                        return Ok(committees);
                    }
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
        #endregion

        #region Task Global Search
        [HttpPost("globalSearchTasksPaged/{page}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GlobalSearchTasksPaged(
                                                            int? page,
                                                            [FromQuery] int listType,
                                                            [FromQuery] FilterActionEnum taskFilterAction,
                                                            [FromBody] TaskSearchRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.TaskSearch))
                {
                    return BadRequest(new
                    {
                        error = "Missing search term",
                        message = "Search term is required to perform the search."
                    });
                }

                var validationResult = SearchTermSecurityValidator.ValidateSearchTerm(request.TaskSearch);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new
                    {
                        error = "Invalid search term",
                        message = "Search failed: input contains characters or patterns not allowed for security reasons.",
                        details = validationResult.ViolatedPattern?.Description
                    });
                }

                var uid = Request.Headers.TryGetValue("uid", out var uidHeader) ? uidHeader.ToString() : null;

                var result = await _taskQueries.GetAllTasksContentPaginated(
                    page,
                    listType,
                    validationResult.DecodedTerm,
                    Response,
                    taskFilterAction,
                    uid);

                var encodedData = ResponseMessage.EncodeData(result ?? new List<GlobalSearchTaskDto>());

                return Ok(new EncodedResponse<IList<GlobalSearchTaskDto>>
                {
                    Data = encodedData,
                    IsEncoded = true
                });
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    error = "Internal server error",
                    message = "An unexpected error occurred while processing the request."
                });
            }
        }
        #endregion
    }
}

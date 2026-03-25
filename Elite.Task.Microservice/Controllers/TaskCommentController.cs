using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Elite_Task.Microservice.Application.CQRS.Queries;
using System.Net;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Newtonsoft.Json;
using Microsoft.Extensions.Primitives;
using Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto;
using Elite.Task.Microservice.Application.CQRS.Queries;
using Elite.Task.Microservice.Application.CQRS.ExternalService;

namespace Elite.Task.Microservice.Controllers
{
    [Route("api/[controller]")]
    public class TaskCommentController : ControllerBase
    {


        #region private Variable
        private readonly IMediator _mediator;
        private readonly ITaskCommentQueries _taskQueries;
        private readonly ITaskLog _taskLog;
        #endregion Variable


        #region Constructor
        public TaskCommentController(IMediator mediator, ITaskCommentQueries taskQueries, ITaskLog taskLog)
        {
            _mediator = mediator;
            _taskQueries = taskQueries;
            _taskLog = taskLog;
        }

        #endregion



        #region comment save
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Post([FromBody] TaskCommentCommand command)
        {
            try
            {
                if (command is null)
                    return BadRequest();

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
                return null;
            }

        }
        #endregion



        #region comment update
        [HttpPut]
        [Route("Update")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Put([FromBody] TaskCommentCommand comment)
        {
            long commandResult = 0;
            if (comment is null || comment.Id < 1)
            {
                return BadRequest();
            }
            try
            {
                comment = await FilterDupicateAttachment(comment);
                commandResult = await _mediator.Send(comment);
                return commandResult > 0 ? (IActionResult)Ok(commandResult) : (IActionResult)BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                if (ex.Message.ToLower() == "unauthorized")
                {
                    return NotFound();
                }
                return null;
            }

        }
        #endregion



        #region get all comments
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetComments(long id)
        {
            try
            {
                if (id > 0)
                {
                    var comments = await _taskQueries.GetCommentsByTaskID(id);
                    if (comments?.Count > 0)
                        return Ok(comments);
                }
                return NotFound();
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

        private async Task<TaskCommentCommand> FilterDupicateAttachment(TaskCommentCommand taskCommentCommand)
        {
            if (taskCommentCommand.TaskId.HasValue && taskCommentCommand.Attachments != null && taskCommentCommand.Attachments.Count() > 0)
            {
                var listOfAttachmentGuid = await _taskQueries.GetTaskAttachments(taskCommentCommand.Id);

                if (listOfAttachmentGuid != null && listOfAttachmentGuid.Count > 0)
                    foreach (var item in listOfAttachmentGuid)
                    {
                        var attachment = (from p in taskCommentCommand.Attachments
                                          where (p.AttachmentGuid == item && p.IsDeleted != true)
                                          select p).FirstOrDefault();

                        if (attachment != null)
                            taskCommentCommand.Attachments.Remove(attachment);
                    }
                return taskCommentCommand;
            }
            return taskCommentCommand;
        }

    }
}

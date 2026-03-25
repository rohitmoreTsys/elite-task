using Elite.Common.Utilities.CommonType;
using Elite.Common.Utilities.RequestContext;
using Elite.Task.Microservice.Application.CQRS.Commands;
using Elite.Task.Microservice.Application.CQRS.ExternalService;
using Elite.Topic.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Commands;
using Elite_Task.Microservice.Application.CQRS.Queries;
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
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Controllers
{
    [Route("api/[controller]")]
    public class TaskAttachmentController : Controller
    {
        #region private Variable
        private readonly IMediator _mediator;
        private readonly ITaskLog _taskLog;

        private readonly ITaskQueries _taskQueries;
        #endregion Variable

        #region Constructor
        public TaskAttachmentController(IMediator mediator,  ITaskLog taskLog, ITaskQueries taskQueries)
        {
            _mediator = mediator;
            _taskLog = taskLog;
            _taskQueries = taskQueries;
        }

        #endregion

        [HttpPut("UpdateTaskAttachment")]
        public async Task<IActionResult> UpdateTaskAttachmentMapping([FromBody] UpdateTaskAttachmentMappingCommand updatetaskAttachmentMapping)
        {
            long commandResult = 0;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                commandResult = await _mediator.Send(updatetaskAttachmentMapping);

            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return (IActionResult)Ok(commandResult);
            }
            return (IActionResult)Ok(commandResult);
        }

        [HttpGet("GetAllTaskAttachment")]
        public async Task<IActionResult> GetAllTaskAttachment()
        {
            try
            {
                        var tasks = await _taskQueries.GetTaskAttachments();

                        if (tasks?.Count > 0)
                            return Ok(tasks);
                        return BadRequest();
            }
            catch (Exception ex)
            {
                _taskLog.LogTaskError(ex);
                return BadRequest();
            }
        }
    }
}

using Elite.Common.Utilities.RequestContext;
using Elite.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
	public class TaskLog : ITaskLog
	{
		private readonly ILogException _logException;
		private readonly IRequestContext _requestContext;

		public TaskLog()
		{
		}
		public TaskLog(ILogException logException, IRequestContext reqContext)
		{
			_logException = logException;
			_requestContext = reqContext;
		}

		public void LogTaskError(Exception ex)
		{
			try
			{
				_logException.LogEliteError(this._requestContext.UID, ex);
			}
			catch (Exception)
			{
				//Skip the errors.		
			}
		}
	}
}

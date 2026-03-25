using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.ExternalService
{
	public interface ITaskLog
	{
		void LogTaskError(Exception ex);
	}
}

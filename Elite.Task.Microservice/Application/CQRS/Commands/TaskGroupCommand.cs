using Elite_Task.Microservice.Application.CQRS.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Commands
{
	public class TaskGroupCommand
	{
		public string Uid { get; set; }
		public string DisplayName { get; set; }
		public List<TaskPersonCommand> users { get; set; }
	}
}

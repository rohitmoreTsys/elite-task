using Elite_Task.Microservice.Application.CQRS.Queries.QueriesDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite.Task.Microservice.Application.CQRS.Queries.QueriesDto
{
	public class QueriesGroupDto
	{
		public string Uid { get; set; }
		public string DisplayName { get; set; }
		public List<QueriesPersonDto> Users { get; set; }
	}
}

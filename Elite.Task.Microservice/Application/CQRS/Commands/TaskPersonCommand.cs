using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Elite_Task.Microservice.Application.CQRS.Commands
{
    public class TaskPersonCommand
    {
		public TaskPersonCommand(string uid, string displayName)
		{
			Uid = uid;
			DisplayName = displayName;
		}

		public string Uid { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Title { get; set; }
    }
    //used for displaying in the Task template
    public class TaskPeople
    {
        public TaskPersonCommand Responsible { get; set; }
        public List<TaskPersonCommand> CoResponsibles { get; set; }

        public bool IsUpdateOperation { get; set; }

        public TaskPeople(TaskPersonCommand responsible, List<TaskPersonCommand> coResponsibles)
        {
            Responsible = responsible;
            CoResponsibles = coResponsibles;
            IsUpdateOperation = false; // default
        }

        public TaskPeople(TaskPersonCommand responsible, List<TaskPersonCommand> coResponsibles, bool isUpdateOperation)
        {
            Responsible = responsible;
            CoResponsibles = coResponsibles;
            IsUpdateOperation = isUpdateOperation;
        }
    }

    public class AgendaTitleDetails
    {
        public long Id { get; set; }
        public long TopicId { get; set; }
        public string Title { get; set; }
        public string TitleInGerman { get; set; }
    }

}

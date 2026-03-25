using Elite.Task.Microservice.Application.SearchFilter;
using System;
using System.Collections.Generic;

namespace Elite_Task.Microservice.Application.SearchFilter
{
    public class TaskSearchKeywords
    {
        public TaskSearchKeywords()
        {
            Responsible = new List<string>();
            CoResponsibles = new List<string>();
        }

        public long? Id { get; set; }

        public string TaskTitle { get; set; }

        public List<string> Responsible { get; set; }
        public List<Person> ResponsibleJson { get; set; }

        public List<string> CoResponsibles { get; set; }
        public List<Person> CoResponsiblesJson { get; set; }

        public DateTime? DueStartDate { get; set; }

        public DateTime? DueEndDate { get; set; }

        public List<long?> CommitteeId { get; set; }

        public List<bool> TaskType { get; set; }

        public bool savefilter { get; set; }
        public bool isClearSearch { get; set; }

        public TaskFilterType TaskFilterType { get; set; }

        public DateTime? MeetingStartDate { get; set; }

        public DateTime? MeetingEndDate { get; set; }
        public List<string> ResponsibleDivision { get; set; }

    }

    public class Person
    {
        public string uid { get; set; }

        public string displayName { get; set; }

    }

    public class TaskSearchRequest
    {
        public string TaskSearch { get; set; }
    }


}

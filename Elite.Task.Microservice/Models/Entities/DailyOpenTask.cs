using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Elite.Task.Microservice.Models.Entities
{
    public class DailyOpenTask
    {
        
        [Column("task_date")]
        public DateTime TaskDate { get; set; }
        [Column("committee_id")]
        public long CommitteeId { get; set; }
        [Column("open_tasks_count")]
        public int OpenTasksCount { get; set; }
        public string Division { get; set; }
    }

    public class ChartDataPoint
    {
        public string Date { get; set; } // "dd-MM-yyyy"
        public string Values { get; set; } // keep as string for Chart.js
    }

    public class CommitteeTaskSeries
    {
        public string Committee { get; set; } // "comm X"
        public string Division { get; set; }
        public List<ChartDataPoint> Data { get; set; }
    }

}

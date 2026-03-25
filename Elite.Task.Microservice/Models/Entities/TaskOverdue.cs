using System.ComponentModel.DataAnnotations;

namespace Elite.Task.Microservice.Models.Entities
{
    public class TaskOverdueSummary
    {
        //[Key]
        //public long? CommitteeID { get; set; }
        [Key]
        public string Division { get; set; }
        public long OnTime { get; set; }
        public long MonthsOverdue1to3 { get; set; }
        public long MonthsOverdue4to6 { get; set; }
        public long MonthsOverdueMorethan6 { get; set; }

    }
}

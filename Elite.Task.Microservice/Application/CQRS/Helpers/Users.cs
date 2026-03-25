namespace Elite.Task.Microservice.Application.CQRS.Helpers
{
    public class Users
    {
        public long UserId { get; set; }
        public string Uid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string DisplayName { get; set; }
        public bool? IsNonMBParticipant { get; set; }
        public string Title { get; set; }
    }
}

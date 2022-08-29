namespace HITs_classroom.Models.Invitation
{
    public class InvitationDB
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public int Role { get; set; }
        public bool IsAccepted { get; set; }
    }
}

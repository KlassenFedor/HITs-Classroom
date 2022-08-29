namespace HITs_classroom.Models.Invitation
{
    public class InvitationManagement
    {
        public string UserId { get; set; }
        public string CourseId { get; set; }
        public int Role { get; set; }
        public bool IsAccepted { get; set; }
    }
}

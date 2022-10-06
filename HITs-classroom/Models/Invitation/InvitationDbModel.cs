using HITs_classroom.Models.Course;

namespace HITs_classroom.Models.Invitation
{
    public class InvitationDbModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string CourseId { get; set; }
        public int Role { get; set; }
        public bool IsAccepted { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset UpdateTime { get; set; }
        public CourseDbModel Course { get; set; }
    }
}

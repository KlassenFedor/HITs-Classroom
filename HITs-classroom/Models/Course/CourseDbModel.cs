using HITs_classroom.Models.Invitation;
using HITs_classroom.Models.User;

namespace HITs_classroom.Models.Course
{
    public class CourseDbModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Section { get; set; }
        public string? Description { get; set; }
        public string? DescriptionHeading { get; set; }
        public string? EnrollmentCode { get; set; }
        public string? Room { get; set; }
        public int CourseState { get; set; }
        public bool HasAllTeachers { get; set; }
        public List<InvitationDbModel> Invitations { get; set; }
    }
}

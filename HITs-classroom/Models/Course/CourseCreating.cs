using Google.Apis.Classroom.v1;

namespace HITs_classroom.Models.Course
{
    public class CourseCreating
    {
        public string Name { get; set; }
        public string? Section { get; set; }
        public string? DescriptionHeading { get; set; }
        public string? Description { get; set; }
        public string? Room { get; set; }
        public string OwnerId { get; set; }
        public string? CourseState { get; set; }
    }
}

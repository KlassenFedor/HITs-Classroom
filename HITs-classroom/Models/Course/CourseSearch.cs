using Google.Apis.Classroom.v1;

namespace HITs_classroom.Models.Course
{
    public class CourseSearch
    {
        public string? StudentId { get; set; }
        public string? TeacherId { get; set; }
        public string? CourseState { get; set; } 
    }
}

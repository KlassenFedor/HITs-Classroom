using HITs_classroom.Models.CoursesList;

namespace HITs_classroom.Models.Task
{
    public class AssignedTask
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public DateTimeOffset CreationTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
        public List<CoursePreCreatingModel>? RelatedCourses { get; set; }
    }
}

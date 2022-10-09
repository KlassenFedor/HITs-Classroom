using HITs_classroom.Models.Course;
using HITs_classroom.Models.Task;

namespace HITs_classroom.Models.CoursesList
{
    public class CoursePreCreatingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Section { get; set; }
        public string? DescriptionHeading { get; set; }
        public string? Description { get; set; }
        public string? Room { get; set; }
        public int TaskId { get; set; }
        public bool IsCreated { get; set; }
        public CourseDbModel? RealCourse { get; set; }
        public AssignedTask Task { get; set; }

    }
}

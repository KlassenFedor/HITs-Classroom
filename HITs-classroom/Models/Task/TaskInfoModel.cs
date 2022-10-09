using HITs_classroom.Models.Course;

namespace HITs_classroom.Models.Task
{
    public class TaskInfoModel
    {
        public int TaskId { get; set; }
        public string Status { get; set; }
        public int? CoursesCreated { get; set; }
        public int? CoursesAssigned { get; set; }
        public List<CourseNameAndIdModel>? Courses { get; set; }
    }
}

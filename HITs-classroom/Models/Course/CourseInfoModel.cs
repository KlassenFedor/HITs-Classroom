namespace HITs_classroom.Models.Course
{
    public class CourseInfoModel
    {
        public string CourseId { get; set; }
        public string Name { get; set; }
        public string? Section { get; set; }
        public string? DescriptionHeading { get; set; }
        public string? Description { get; set; }
        public string? Room { get; set; }
        public string? EnrollmentCode { get; set; }
        public bool? HasAllTeachers { get; set; }
        public string? CourseState { get; set; }
    }
}

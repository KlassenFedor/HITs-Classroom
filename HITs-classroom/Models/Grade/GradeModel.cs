namespace HITs_classroom.Models.Grade
{
    public class GradeModel
    {
        public string CourseId { get; set; }
        public string CourseWorkId { get; set; }
        public string WorkName { get; set; }
        public string StudentId { get; set; }
        public string StudentEmail { get; set; }
        public double? DraftGrade { get; set; }
        public double? AssignedGrade { get; set; }
    }
}

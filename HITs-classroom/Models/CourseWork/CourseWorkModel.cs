namespace HITs_classroom.Models.CourseWork
{
    public class CourseWorkModel
    {
        public string CourseId { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public int? WorksEvaluted { get; set; }
        public int? WorksPassed { get; set; }
        public int? WorksAssigned { get; set; }
    }
}

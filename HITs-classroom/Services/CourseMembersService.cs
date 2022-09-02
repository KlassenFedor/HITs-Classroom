using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;

namespace HITs_classroom.Services
{
    public interface ICourseMembersService
    {
        List<Student> GetStudentsList(string courseId);
        public List<Teacher> GetTeachersList(string courseId);
        public void DeleteStudent(string courseId, string studentId);
        public void DeleteTeacher(string courseId, string teacherId);
    }
    public class CourseMembersService: ICourseMembersService
    {
        private GoogleClassroomService _googleClassroomService;
        public CourseMembersService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }

        public List<Student> GetStudentsList(string courseId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();

            string pageToken = null;
            List<Student> students = new List<Student>();
            do
            {
                var request = classroomService.Courses.Students.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                students.AddRange(response.Students);
                pageToken = response.NextPageToken;
            } while (pageToken != null);
            
            return students;
        }

        public List<Teacher> GetTeachersList(string courseId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();

            string pageToken = null;
            List<Teacher> teachers = new List<Teacher>();
            do
            {
                var request = classroomService.Courses.Teachers.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                teachers.AddRange(response.Teachers);
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return teachers;
        }

        public void DeleteStudent(string courseId, string studentId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var request = classroomService.Courses.Students.Delete(courseId, studentId);
            var response = request.Execute();
        }

        public void DeleteTeacher(string courseId, string teacherId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var request = classroomService.Courses.Teachers.Delete(courseId, teacherId);
            var response = request.Execute();
        }
    }
}

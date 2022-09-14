using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.ClassrooomUser;

namespace HITs_classroom.Services
{
    public interface ICourseMembersService
    {
        List<UserInfoModel> GetStudentsList(string courseId, string relatedUser);
        public List<UserInfoModel> GetTeachersList(string courseId, string relatedUser);
        public void DeleteStudent(string courseId, string studentId, string relatedUser);
        public void DeleteTeacher(string courseId, string teacherId, string relatedUser);
    }
    public class CourseMembersService: ICourseMembersService
    {
        private GoogleClassroomService _googleClassroomService;
        public CourseMembersService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }

        public List<UserInfoModel> GetStudentsList(string courseId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            string pageToken = null;
            List<Student> students = new List<Student>();
            do
            {
                var request = classroomService.Courses.Students.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                if (response.Students != null)
                {
                    students.AddRange(response.Students);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            List<UserInfoModel> users = new List<UserInfoModel>();
            users = students.Select(s => new UserInfoModel { 
                UserId = s.UserId,
                Email = s.Profile.EmailAddress,
                Name = s.Profile.Name.FullName
            }).ToList();

            return users;
        }

        public List<UserInfoModel> GetTeachersList(string courseId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            string pageToken = null;
            List<Teacher> teachers = new List<Teacher>();
            do
            {
                var request = classroomService.Courses.Teachers.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
                if (response.Teachers != null)
                {
                    teachers.AddRange(response.Teachers);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            List<UserInfoModel> users = new List<UserInfoModel>();
            users = teachers.Select(t => new UserInfoModel
            {
                UserId = t.UserId,
                Email = t.Profile.EmailAddress,
                Name = t.Profile.Name.FullName
            }).ToList();

            return users;
        }

        public void DeleteStudent(string courseId, string studentId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            var request = classroomService.Courses.Students.Delete(courseId, studentId);
            var response = request.Execute();
        }

        public void DeleteTeacher(string courseId, string teacherId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            var request = classroomService.Courses.Teachers.Delete(courseId, teacherId);
            var response = request.Execute();
        }
    }
}

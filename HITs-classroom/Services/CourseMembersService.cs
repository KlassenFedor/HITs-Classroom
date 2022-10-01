using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.ClassrooomUser;

namespace HITs_classroom.Services
{
    public interface ICourseMembersService
    {
        Task<List<UserInfoModel>> GetStudentsList(string courseId);
        Task<List<UserInfoModel>> GetTeachersList(string courseId);
        Task DeleteStudent(string courseId, string studentId);
        Task DeleteTeacher(string courseId, string teacherId);
    }
    public class CourseMembersService: ICourseMembersService
    {
        private ClassroomService _service;
        public CourseMembersService(GoogleClassroomServiceForServiceAccount googleClassroomServiceForServiceAccount)
        {
            _service = googleClassroomServiceForServiceAccount.GetClassroomService();
        }
        public async Task<List<UserInfoModel>> GetStudentsList(string courseId)
        {
            string pageToken = null;
            List<Student> students = new List<Student>();
            do
            {
                var request = _service.Courses.Students.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync();
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

        public async Task<List<UserInfoModel>> GetTeachersList(string courseId)
        {
            string pageToken = null;
            List<Teacher> teachers = new List<Teacher>();
            do
            {
                var request = _service.Courses.Teachers.List(courseId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync();
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

        public async Task DeleteStudent(string courseId, string studentId)
        {
            var request = _service.Courses.Students.Delete(courseId, studentId);
            var response = await request.ExecuteAsync();
        }

        public async Task DeleteTeacher(string courseId, string teacherId)
        {
            var request = _service.Courses.Teachers.Delete(courseId, teacherId);
            var response = await request.ExecuteAsync();
        }
    }
}

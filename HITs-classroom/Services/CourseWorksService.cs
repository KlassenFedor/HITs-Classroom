using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.ClassrooomUser;
using HITs_classroom.Models.Grade;

namespace HITs_classroom.Services
{
    public interface ICourseWorksService
    {
        Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId, ClassroomService service);
        Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, ClassroomService service);
    }

    public class CourseWorksService: ICourseWorksService
    {
        public async Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId, ClassroomService service)
        {
            string pageToken = null;
            List<StudentSubmission> submissions = new List<StudentSubmission>();
            do
            {
                var request = service.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = await request.ExecuteAsync();
                if (response.StudentSubmissions != null)
                {
                    submissions.AddRange(response.StudentSubmissions);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            List<GradeModel> grades = new List<GradeModel>();
            foreach (var submission in submissions)
            {
                GradeModel gradeModel = new GradeModel();
                gradeModel.CourseId = courseId;
                gradeModel.CourseWorkId = courseWorkId;
                gradeModel.StudentId = submission.UserId;
                gradeModel.Grade = submission.AssignedGrade;
                gradeModel.WorkName = "name";
                UserInfoModel userInfoModel = GetUserInfo(submission.UserId, service);
                if (userInfoModel != null)
                {
                    gradeModel.StudentEmail = userInfoModel.Email;
                }
            }

            return grades;
        }

        public async Task<List<List<GradeModel>>> GedCourseGrades(string courseId, string relatedUser)
        {
            return null;
        }

        public async Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, ClassroomService service)
        {
            ModifyCourseWorkAssigneesRequest courseWorkModification = new ModifyCourseWorkAssigneesRequest();
            courseWorkModification.AssigneeMode = AssigneeModeEnum.INDIVIDUAL_STUDENTS.ToString();
            courseWorkModification.ModifyIndividualStudentsOptions.AddStudentIds = users;
            var request = service.Courses.CourseWork.ModifyAssignees(courseWorkModification, courseId, courseWorkId);
            var response = await request.ExecuteAsync();
        }

        private UserInfoModel GetUserInfo(string UserId, ClassroomService service)
        {
            var request = service.UserProfiles.Get(UserId);
            var response = request.Execute();

            if (response != null)
            {
                UserInfoModel user = new UserInfoModel();
                user.UserId = response.Id;
                user.Email = response.EmailAddress;
                user.Name = response.Name.FullName;

                return user;
            }

            return null;
        }
    }
}

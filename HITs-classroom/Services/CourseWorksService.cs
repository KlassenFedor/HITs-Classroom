using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.ClassrooomUser;
using HITs_classroom.Models.Grade;

namespace HITs_classroom.Services
{
    public interface ICourseWorksService
    {
        Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId, string relatedUser);
        Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser);
    }

    public class CourseWorksService: ICourseWorksService
    {
        private GoogleClassroomService _googleClassroomService;
        public CourseWorksService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }
        public async Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            string pageToken = null;
            List<StudentSubmission> submissions = new List<StudentSubmission>();
            do
            {
                var request = classroomService.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
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
                UserInfoModel userInfoModel = GetUserInfo(submission.UserId, relatedUser);
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

        public async Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            ModifyCourseWorkAssigneesRequest courseWorkModification = new ModifyCourseWorkAssigneesRequest();
            courseWorkModification.AssigneeMode = AssigneeModeEnum.INDIVIDUAL_STUDENTS.ToString();
            courseWorkModification.ModifyIndividualStudentsOptions.AddStudentIds = users;
            var request = classroomService.Courses.CourseWork.ModifyAssignees(courseWorkModification, courseId, courseWorkId);
            var response = await request.ExecuteAsync();
        }

        private UserInfoModel GetUserInfo(string UserId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            var request = classroomService.UserProfiles.Get(UserId);
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

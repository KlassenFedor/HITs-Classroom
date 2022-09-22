using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.Grade;
using HITs_classroom.Models.User;

namespace HITs_classroom.Services
{
    public interface ICourseWorksService
    {
        List<GradeModel> GetGradesForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser);
        void SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser);
    }

    public class CourseWorksService: ICourseWorksService
    {
        private GoogleClassroomService _googleClassroomService;
        public CourseWorksService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }
        public List<GradeModel> GetGradesForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            string pageToken = null;
            List<StudentSubmission> submissions = new List<StudentSubmission>();
            do
            {
                var request = classroomService.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
                request.PageSize = 100;
                request.PageToken = pageToken;
                var response = request.Execute();
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
                UserInfoModel userInfoModel = GetUserInfo(submission.UserId, relatedUser);
                if (userInfoModel != null)
                {
                    gradeModel.StudentEmail = userInfoModel.Email;
                }
            }

            foreach (var grade in grades)
            {
                if (grade.StudentEmail == null || !users.Contains(grade.StudentEmail))
                {
                    grades.Remove(grade);
                }
            }

            return grades;
        }
        public void SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);

            ModifyCourseWorkAssigneesRequest courseWorkModification = new ModifyCourseWorkAssigneesRequest();
            courseWorkModification.AssigneeMode = AssigneeModeEnum.INDIVIDUAL_STUDENTS.ToString();
            courseWorkModification.ModifyIndividualStudentsOptions.AddStudentIds = users;
            var request = classroomService.Courses.CourseWork.ModifyAssignees(courseWorkModification, courseId, courseWorkId);
            var response = request.Execute();
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

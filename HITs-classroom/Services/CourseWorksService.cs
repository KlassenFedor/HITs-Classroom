using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Enums;
using HITs_classroom.Models.ClassrooomUser;
using HITs_classroom.Models.CourseWork;
using HITs_classroom.Models.Grade;

namespace HITs_classroom.Services
{
    public interface ICourseWorksService
    {
        Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId);
        Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users);
        Task<Dictionary<string, List<GradeModel>>> GetCourseGrades(string courseId);
        Task<List<CourseWorkModel>> GetCourseWorks(string courseId);
    }

    public class CourseWorksService: ICourseWorksService
    {
        private ClassroomService _service;
        public CourseWorksService(GoogleClassroomServiceForServiceAccount googleClassroomServiceForServiceAccount)
        {
            _service = googleClassroomServiceForServiceAccount.GetClassroomService();
        }
        public async Task<List<GradeModel>> GetGradesForCourseWork(string courseId, string courseWorkId)
        {
            string pageToken = null;
            List<StudentSubmission> submissions = new List<StudentSubmission>();
            do
            {
                var request = _service.Courses.CourseWork.StudentSubmissions.List(courseId, courseWorkId);
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
                GradeModel gradeModel = CreateGradeModelFromStudentSubmission(submission);
                grades.Add(gradeModel);
            }

            return grades;
        }

        public async Task<Dictionary<string, List<GradeModel>>> GetCourseGrades(string courseId)
        {
            var courseWorks = await _service.Courses.CourseWork.List(courseId).ExecuteAsync();
            Dictionary<string, List<GradeModel>> grades = new Dictionary<string, List<GradeModel>>();
            foreach (CourseWork courseWork in courseWorks.CourseWork)
            {
                grades.Add(courseWork.Title, await GetGradesForCourseWork(courseId, courseWork.Id));
            }
            return grades;
        }

        public async Task SetAdmittedStudentsForCourseWork(string courseId, string courseWorkId, List<string>? users)
        {
            ModifyCourseWorkAssigneesRequest courseWorkModification = new ModifyCourseWorkAssigneesRequest();
            courseWorkModification.AssigneeMode = AssigneeModeEnum.INDIVIDUAL_STUDENTS.ToString();
            courseWorkModification.ModifyIndividualStudentsOptions.AddStudentIds = users;
            var request = _service.Courses.CourseWork.ModifyAssignees(courseWorkModification, courseId, courseWorkId);
            var response = await request.ExecuteAsync();
        }

        public async Task<List<CourseWorkModel>> GetCourseWorks(string courseId)
        {
            var response = await _service.Courses.CourseWork.List(courseId).ExecuteAsync();
            var works = response.CourseWork;
            List<CourseWorkModel> courseWorks = new List<CourseWorkModel>();
            foreach (var work in works)
            {
                CourseWorkModel courseWork = new CourseWorkModel();
                courseWork.CourseId = work.CourseId;
                courseWork.Id = work.Id;
                courseWork.Title = work.Title;
                courseWorks.Add(courseWork);
            }

            return courseWorks;
        }

        //private CourseWorkModel CreateCourseWorkModel(CourseWork courseWork)
        //{
        //    var courseWorkModel = new CourseWorkModel();
        //    courseWorkModel.CourseId = courseWork.CourseId;
        //    courseWorkModel.Id = courseWork.Id;
        //    courseWorkModel.
        //}

        private GradeModel CreateGradeModelFromStudentSubmission(StudentSubmission submission)
        {
            GradeModel gradeModel = new GradeModel();
            gradeModel.CourseId = submission.CourseId;
            gradeModel.CourseWorkId = submission.CourseWorkId;
            gradeModel.StudentId = submission.UserId;
            gradeModel.DraftGrade = submission.DraftGrade;
            gradeModel.AssignedGrade = submission.AssignedGrade;
            UserInfoModel userInfoModel = GetUserInfo(submission.UserId);
            if (userInfoModel != null)
            {
                gradeModel.StudentEmail = userInfoModel.Email;
            }
            return gradeModel;
        }


        private UserInfoModel GetUserInfo(string UserId)
        {
            var request = _service.UserProfiles.Get(UserId);
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

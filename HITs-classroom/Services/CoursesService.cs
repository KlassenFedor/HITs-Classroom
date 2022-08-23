using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace HITs_classroom.Services
{
    public interface ICoursesService
    {
        //Task CreateCourseAsync();
        Course GetCourse(string courseId);
        List<Course> GetCoursesList();
        List<Course> GetActiveCoursesList();
        List<Course> GetArchivedCoursesList();
    }

    public class CoursesService: ICoursesService
    {
        private GoogleClassroomService _googleClassroomService;
        public CoursesService(GoogleClassroomService googleClassroomService)
        {
            _googleClassroomService = googleClassroomService;
        }

        public Course GetCourse(string courseId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            Course course = classroomService.Courses.Get(courseId).Execute();
            return course;
        }

        public List<Course> GetCoursesList()
        {
            return null;
        }

        public List<Course> GetActiveCoursesList()
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ACTIVE;
                var response = request.Execute();
                courses.AddRange(response.Courses);
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }

        public List<Course> GetArchivedCoursesList()
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.CourseStates = CoursesResource.ListRequest.CourseStatesEnum.ARCHIVED;
                var response = request.Execute();
                courses.AddRange(response.Courses);
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }
    }
}

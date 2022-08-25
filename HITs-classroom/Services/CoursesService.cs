using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using HITs_classroom.Models.Course;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Reflection;
using System.Text.Json;

namespace HITs_classroom.Services
{
    public interface ICoursesService
    {
        Course GetCourse(string courseId);
        List<Course> GetCoursesList(CourseSearch parameters);
        List<Course> GetActiveCoursesList();
        List<Course> GetArchivedCoursesList();
        Course CreateCourse(CourseShortModel course);
        string DeleteCourse(string courseId);
        Course PatchCourse(string courseId, CoursePatching parameters);
        Course UpdateCourse(string courseId, CoursePatching parameters);
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

        public List<Course> GetCoursesList(CourseSearch parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.StudentId = parameters.StudentId;
                request.TeacherId = parameters.TeacherId;
                CoursesResource.ListRequest.CourseStatesEnum status;
                Enum.TryParse(parameters.CourseState, out status);
                request.CourseStates = status;
                var response = request.Execute();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
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
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
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
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }

        public Course CreateCourse(CourseShortModel parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            var newCourse = new Course
            {
                Name = parameters.Name,
                Section = parameters.Section,
                DescriptionHeading = parameters.DescriptionHeading,
                Description = parameters.Description,
                Room = parameters.Room,
                OwnerId = parameters.OwnerId,
                CourseState = parameters.CourseState
            };

            newCourse = classroomService.Courses.Create(newCourse).Execute();
            return newCourse;
        }

        public List<Course> CreateCoursesList(List<CourseShortModel> courses)
        {
            ClassroomService classroomService;
            try
            {
                classroomService = _googleClassroomService.GetClassroomService();
                List<Course> result = new List<Course>();

                foreach (var courseShortModel in courses)
                {
                    var newCourse = new Course
                    {
                        Name = courseShortModel.Name,
                        Section = courseShortModel.Section,
                        DescriptionHeading = courseShortModel.DescriptionHeading,
                        Description = courseShortModel.Description,
                        Room = courseShortModel.Room,
                        OwnerId = courseShortModel.OwnerId,
                        CourseState = courseShortModel.CourseState
                    };

                    try
                    {
                        Course newExistingCourse = classroomService.Courses.Create(newCourse).Execute();
                        result.Add(newExistingCourse);
                    }
                    catch
                    {
                        newCourse.CourseState = CoursesResource.ListRequest.CourseStatesEnum.COURSESTATEUNSPECIFIED.ToString();
                        result.Add(newCourse);
                    }
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        public string DeleteCourse(string courseId)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();

            var response = JsonSerializer.Serialize(classroomService.Courses.Delete(courseId).Execute());
            return response;
        }

        public Course PatchCourse(string courseId, CoursePatching parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            string updateMask = MakeMaskFromCourseModel(parameters);

            Course course = new Course
            {
                Name = parameters.Name,
                OwnerId = parameters.OwnerId,
                Section = parameters.Section,
                CourseState = parameters.CourseState,
                Room = parameters.Room,
                DescriptionHeading = parameters.DescriptionHeading,
                Description = parameters.Description
            };
            var request = classroomService.Courses.Patch(course, courseId);
            request.UpdateMask = updateMask;
            course = request.Execute();
            return course;
        }

        public Course UpdateCourse(string courseId, CoursePatching parameters)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService();
            Course course = classroomService.Courses.Get(courseId).Execute();
            if (parameters.Name != null)
            {
                course.Name = parameters.Name;
            }
            if (parameters.OwnerId != null)
            {
                course.OwnerId = parameters.OwnerId;
            }
            if (parameters.Section != null)
            {
                course.Section = parameters.Section;
            }
            if (parameters.Room != null)
            {
                course.Room = parameters.Room;
            }
            if (parameters.Description != null)
            {
                course.Description = parameters.Description;
            }
            if (parameters.DescriptionHeading != null)
            {
                course.DescriptionHeading = parameters.DescriptionHeading;
            }
            if (parameters.CourseState != null)
            {
                course.Description = parameters.Description;
            }
            course = classroomService.Courses.Update(course, courseId).Execute();
            return course;
        }

        private string MakeMaskFromCourseModel(CoursePatching model)
        {
            string result = "";
            if (model.Name != null)
            {
                result = result + "name,";
            }
            if (model.OwnerId != null)
            {
                result = result + "ownerId,";
            }
            if (model.Section != null)
            {
                result = result + "section,";
            }
            if (model.Room != null)
            {
                result = result + "room,";
            }
            if (model.Description != null)
            {
                result = result + "description,";
            }
            if (model.DescriptionHeading != null)
            {
                result = result + "descriptionHeading,";
            }
            if (model.CourseState != null)
            {
                result = result + "courseState,";
            }
            if (result[result.Length - 1] == ',')
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
    }
}
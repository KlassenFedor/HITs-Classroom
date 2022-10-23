using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.Course;
using static Google.Apis.Classroom.v1.CoursesResource.ListRequest;

namespace HITs_classroom.Helpers
{
    static class ModelsConverter
    {
        public static CourseInfoModel CreateCourseInfoModelFromCourseDbModel(CourseDbModel courseDb)
        {
            CourseInfoModel courseInfoModel = new CourseInfoModel();
            courseInfoModel.CourseId = courseDb.Id;
            courseInfoModel.Name = courseDb.Name;
            courseInfoModel.Room = courseDb.Room;
            courseInfoModel.Description = courseDb.Description;
            courseInfoModel.DescriptionHeading = courseDb.DescriptionHeading;
            courseInfoModel.Section = courseDb.Section;
            courseInfoModel.EnrollmentCode = courseDb.EnrollmentCode;
            courseInfoModel.CourseState = ((CourseStatesEnum)courseDb.CourseState).ToString();
            courseInfoModel.HasAllTeachers = courseDb.HasAllTeachers;

            return courseInfoModel;
        }

        public static CourseInfoModel CreateCourseInfoModelFromGoogleCourseModel(Course course)
        {
            CourseInfoModel courseInfoModel = new CourseInfoModel();
            courseInfoModel.CourseId = course.Id;
            courseInfoModel.Name = course.Name;
            courseInfoModel.Room = course.Room;
            courseInfoModel.Description = course.Description;
            courseInfoModel.DescriptionHeading = course.DescriptionHeading;
            courseInfoModel.Section = course.Section;
            courseInfoModel.EnrollmentCode = course.EnrollmentCode;
            courseInfoModel.CourseState = course.CourseState;

            return courseInfoModel;
        }
    }
}

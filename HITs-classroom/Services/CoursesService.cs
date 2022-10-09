using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Jobs;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.CoursesList;
using HITs_classroom.Models.Task;
using Microsoft.EntityFrameworkCore;
using static Google.Apis.Classroom.v1.CoursesResource.ListRequest;

namespace HITs_classroom.Services
{
    public interface ICoursesService
    {
        Task<Course> GetCourseFromGoogleClassroom(string courseId);
        Task<CourseInfoModel> GetCourseFromDb(string courseId);
        Task<List<CourseInfoModel>> GetCoursesListFromDb(string? courseState);
        Task<List<CourseInfoModel>> GetActiveCoursesListFromDb();
        Task<List<CourseInfoModel>> GetArchivedCoursesListFromDb();
        Task<CourseInfoModel> CreateCourse(CourseShortModel parameters);
        Task DeleteCourse(string courseId);
        Task<CourseInfoModel> PatchCourse(string courseId, CoursePatching parameters);
        Task<CourseInfoModel> UpdateCourse(string courseId, CoursePatching parameters);
        Task<List<CourseInfoModel>> SynchronizeCoursesListsInDbAndGoogleClassroom();
    }

    public class CoursesService: ICoursesService
    {
        private ApplicationDbContext _context;
        private ClassroomService _service;
        public CoursesService(ApplicationDbContext context,
            GoogleClassroomServiceForServiceAccount googleClassroomServiceForServiceAccount)
        {
            _context = context;
            _service = googleClassroomServiceForServiceAccount.GetClassroomService();
        }

        public async Task<Course> GetCourseFromGoogleClassroom(string courseId)
        {
            Course course = await _service.Courses.Get(courseId).ExecuteAsync();
            return course;
        }

        public async Task<CourseInfoModel> GetCourseFromDb(string courseId)
        {
            CourseDbModel? course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course != null)
            {
                return CreateCourseInfoModelFromCourseDbModel(course);
            }
            throw new NullReferenceException();
        }

        private async Task<List<Course>> GetCoursesListFromGoogleClassroom(CourseSearch parameters)
        {
            string? pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = _service.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.StudentId = parameters.StudentId;
                request.TeacherId = parameters.TeacherId;
                if (parameters.CourseState != null)
                {
                    CourseStatesEnum status;
                    if (Enum.TryParse(parameters.CourseState, out status))
                    {
                        request.CourseStates = status;
                    }
                }
                var response = await request.ExecuteAsync();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }

        public async Task<List<CourseInfoModel>> GetCoursesListFromDb(string? courseState)
        {
            List<CourseDbModel> courseDbModels;
            CourseStatesEnum status;
            if (Enum.TryParse(courseState, out status))
            {
                courseDbModels = await _context.Courses
                    .Where(c => c.CourseState == (int)status).ToListAsync();
            }
            else
            {
                courseDbModels = await _context.Courses.ToListAsync();
            }
            List<CourseInfoModel> courses = courseDbModels.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();

            return courses;
        }

        public async Task<List<CourseInfoModel>> GetActiveCoursesListFromDb()
        {
            List<CourseDbModel> courses = new List<CourseDbModel>();
            courses = await _context.Courses
                .Where(c => c.CourseState == (int)CourseStatesEnum.ACTIVE).ToListAsync();
            return courses.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();
        }

        public async Task<List<CourseInfoModel>> GetArchivedCoursesListFromDb()
        {
            List<CourseDbModel> courses = new List<CourseDbModel>();
            courses = await _context.Courses
                .Where(c => c.CourseState == (int)CourseStatesEnum.ARCHIVED).ToListAsync();
            return courses.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();
        }

        public async Task<CourseInfoModel> CreateCourse(CourseShortModel parameters)
        {
            string ownerId = parameters.OwnerId != null ? parameters.OwnerId : "me";
            var newCourse = new Course
            {
                Name = parameters.Name,
                Section = parameters.Section,
                DescriptionHeading = parameters.DescriptionHeading,
                Description = parameters.Description,
                Room = parameters.Room,
                OwnerId = ownerId,
                CourseState = parameters.CourseState
            };

            newCourse = await _service.Courses.Create(newCourse).ExecuteAsync();
            CourseDbModel courseDb = new CourseDbModel
            {
                Id = newCourse.Id,
                Name = newCourse.Name,
                Section = newCourse.Section,
                Description = newCourse.Description,
                DescriptionHeading = newCourse.DescriptionHeading,
                Room = newCourse.Room,
                EnrollmentCode = newCourse.EnrollmentCode,
                CourseState = (int)Enum.Parse<CourseStatesEnum>("ACTIVE"),
                HasAllTeachers = true
            };

            await _context.Courses.AddAsync(courseDb);
            await _context.SaveChangesAsync();

            await UpdateCourse(newCourse.Id, new CoursePatching { CourseState = "ACTIVE"});

            return CreateCourseInfoModelFromCourseDbModel(courseDb);

        }

        public async Task DeleteCourse(string courseId)
        {
            var response = await _service.Courses.Delete(courseId).ExecuteAsync();
            CourseDbModel? course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CourseInfoModel> PatchCourse(string courseId, CoursePatching parameters)
        {
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
            var request = _service.Courses.Patch(course, courseId);
            request.UpdateMask = updateMask;
            course = await request.ExecuteAsync();

            CourseDbModel? courseDb = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (courseDb != null)
            {
                if (parameters.Name != null) courseDb.Name = parameters.Name;
                if (parameters.Section != null) courseDb.Section = parameters.Section;
                if (parameters.Description != null) courseDb.Description = parameters.Description;
                if (parameters.DescriptionHeading != null) courseDb.DescriptionHeading = parameters.DescriptionHeading;
                if (parameters.Room != null) courseDb.Room = parameters.Room;
                if (parameters.CourseState != null) courseDb.CourseState = (int)Enum.Parse<CourseStatesEnum>(parameters.CourseState);

                _context.Entry(courseDb).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return CreateCourseInfoModelFromCourseDbModel(courseDb);
            }

            throw new NullReferenceException();
        }

        public async Task<CourseInfoModel> UpdateCourse(string courseId, CoursePatching parameters)
        {
            Course course = _service.Courses.Get(courseId).Execute();
            if (parameters.Name != null) course.Name = parameters.Name;
            if (parameters.OwnerId != null) course.OwnerId = parameters.OwnerId;
            if (parameters.Section != null) course.Section = parameters.Section;
            if (parameters.Room != null) course.Room = parameters.Room;
            if (parameters.Description != null) course.Description = parameters.Description;
            if (parameters.DescriptionHeading != null) course.DescriptionHeading = parameters.DescriptionHeading;
            if (parameters.CourseState != null) course.CourseState = parameters.CourseState;

            course = await _service.Courses.Update(course, courseId).ExecuteAsync();

            CourseDbModel? courseDb = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (courseDb != null)
            {
                courseDb.Name = course.Name;
                courseDb.Section = course.Section;
                courseDb.Room = course.Room;
                courseDb.Description = course.Description;
                courseDb.DescriptionHeading = course.DescriptionHeading;
                courseDb.CourseState = (int)Enum.Parse<CourseStatesEnum>(course.CourseState);

                _context.Entry(courseDb).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return CreateCourseInfoModelFromCourseDbModel(courseDb);
            }

            throw new NullReferenceException();
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

        private CourseInfoModel CreateCourseInfoModelFromCourseDbModel(CourseDbModel courseDb)
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

        private CourseInfoModel CreateCourseInfoModelFromGoogleCourseModel(Course course)
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

        public async Task<List<CourseInfoModel>> SynchronizeCoursesListsInDbAndGoogleClassroom()
        {
            CourseSearch courseSearch = new CourseSearch();
            var courses = await GetCoursesListFromGoogleClassroom(courseSearch);
            List<CourseInfoModel> response = new List<CourseInfoModel>();
            foreach (var course in courses)
            {
                response.Add(await UpdateCourseInDb(course.Id, course));
            }

            return response;
        }

        private async Task<CourseInfoModel> UpdateCourseInDb(string courseId, Course course)
        {
            CourseDbModel? courseDb = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (courseDb == null)
            {
                courseDb = new CourseDbModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Section = course.Section,
                    Description = course.Description,
                    DescriptionHeading = course.DescriptionHeading,
                    Room = course.Room,
                    EnrollmentCode = course.EnrollmentCode,
                    CourseState = (int)Enum.Parse<CourseStatesEnum>(course.CourseState),
                    HasAllTeachers = true
                };

                await _context.Courses.AddAsync(courseDb);
            }
            else
            {
                courseDb.Id = course.Id;
                courseDb.Name = course.Name;
                courseDb.Section = course.Section;
                courseDb.Description = course.Description;
                courseDb.DescriptionHeading = course.DescriptionHeading;
                courseDb.Room = course.Room;
                courseDb.EnrollmentCode = course.EnrollmentCode;
                courseDb.CourseState = (int)Enum.Parse<CourseStatesEnum>(course.CourseState);
            }

            await _context.SaveChangesAsync();

            return CreateCourseInfoModelFromCourseDbModel(courseDb);
        }
    }
}
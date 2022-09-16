using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.User;
using Microsoft.EntityFrameworkCore;
using static Google.Apis.Classroom.v1.CoursesResource.ListRequest;

namespace HITs_classroom.Services
{
    public interface ICoursesService
    {
        Course GetCourseFromGoogleClassroom(string courseId, string relatedUser);
        Task<CourseInfoModel> GetCourseFromDb(string courseId, string user);
        List<CourseInfoModel> GetCoursesListFromGoogleClassroom(CourseSearch parameters, string relatedUser);
        Task<List<CourseInfoModel>> GetCoursesListFromDb(string? courseState, string relatedUser);
        List<Course> GetActiveCoursesListFromGoogleClassroom(string? relatedUser);
        Task<List<CourseInfoModel>> GetActiveCoursesListFromDb(string relatedUser);
        List<Course> GetArchivedCoursesListFromGoogleClassroom(string relatedUser);
        Task<List<CourseInfoModel>> GetArchivedCoursesListFromDb(string relatedUser);
        Task<CourseInfoModel> CreateCourse(CourseShortModel parameters, string relatedUser);
        Task DeleteCourse(string courseId, string relatedUser);
        Task<CourseInfoModel> PatchCourse(string courseId, CoursePatching parameters, string relatedUser);
        Task<CourseInfoModel> UpdateCourse(string courseId, CoursePatching parameters, string relatedUser);
        Task<List<CourseInfoModel>> SynchronizeCoursesListsInDbAndGoogleClassroom(string relatedUser);
    }

    public class CoursesService: ICoursesService
    {
        private GoogleClassroomService _googleClassroomService;
        private ApplicationDbContext _context;
        public CoursesService(GoogleClassroomService googleClassroomService, ApplicationDbContext context)
        {
            _googleClassroomService = googleClassroomService;
            _context = context;
        }

        public Course GetCourseFromGoogleClassroom(string courseId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            Course course = classroomService.Courses.Get(courseId).Execute();
            return course;
        }

        public async Task<CourseInfoModel> GetCourseFromDb(string courseId, string user)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == user);
            if (classroomAdmin != null)
            {
                CourseDbModel? course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.Id == courseId && c.RelatedUsers.Contains(classroomAdmin));
                if (course != null)
                {
                    return CreateCourseInfoModelFromCourseDbModel(course);
                }
            }
            throw new NullReferenceException();
        }

        public List<CourseInfoModel> GetCoursesListFromGoogleClassroom(CourseSearch parameters, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
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
                var response = request.Execute();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            List<CourseInfoModel> courseInfoModels = new List<CourseInfoModel>();
            foreach (var course in courses)
            {
                courseInfoModels.Add(CreateCourseInfoModelFromGoogleCourseModel(course));
            }

            return courseInfoModels;
        }

        public async Task<List<CourseInfoModel>> GetCoursesListFromDb(string? courseState, string relatedUser)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            List<CourseDbModel> courseDbModels;
            CourseStatesEnum status;
            if (Enum.TryParse(courseState, out status))
            {
                courseDbModels = await _context.Courses
                    .Where(c => c.CourseState == (int)status && c.RelatedUsers.Contains(classroomAdmin)).ToListAsync();
            }
            else
            {
                courseDbModels = await _context.Courses.Where(c => c.RelatedUsers.Contains(classroomAdmin)).ToListAsync();
            }
            List<CourseInfoModel> courses = courseDbModels.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();

            return courses;
        }

        public List<Course> GetActiveCoursesListFromGoogleClassroom(string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.CourseStates = CourseStatesEnum.ACTIVE;
                var response = request.Execute();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }
        public async Task<List<CourseInfoModel>> GetActiveCoursesListFromDb(string relatedUser)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            List<CourseDbModel> courses = new List<CourseDbModel>();
            courses = await _context.Courses
                .Where(c => c.CourseState == (int)CourseStatesEnum.ACTIVE && c.RelatedUsers.Contains(classroomAdmin)).ToListAsync();
            return courses.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();
        }

        public List<Course> GetArchivedCoursesListFromGoogleClassroom(string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            string pageToken = null;
            var courses = new List<Course>();

            do
            {
                var request = classroomService.Courses.List();
                request.PageSize = 100;
                request.PageToken = pageToken;
                request.CourseStates = CourseStatesEnum.ARCHIVED;
                var response = request.Execute();
                if (response.Courses != null)
                {
                    courses.AddRange(response.Courses);
                }
                pageToken = response.NextPageToken;
            } while (pageToken != null);

            return courses;
        }

        public async Task<List<CourseInfoModel>> GetArchivedCoursesListFromDb(string relatedUser)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            List<CourseDbModel> courses = new List<CourseDbModel>();
            courses = await _context.Courses
                .Where(c => c.CourseState == (int)CourseStatesEnum.ARCHIVED && c.RelatedUsers.Contains(classroomAdmin)).ToListAsync();
            return courses.Select(c => CreateCourseInfoModelFromCourseDbModel(c)).ToList();
        }

        public async Task<CourseInfoModel> CreateCourse(CourseShortModel parameters, string relatedUser)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
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
            CourseDbModel courseDb = new CourseDbModel
            {
                Id = newCourse.Id,
                Name = newCourse.Name,
                Section = newCourse.Section,
                Description = newCourse.Description,
                DescriptionHeading = newCourse.DescriptionHeading,
                Room = newCourse.Room,
                CourseState = (int)Enum.Parse<CourseStatesEnum>(newCourse.CourseState),
                HasAllTeachers = true
            };
            
            await _context.Courses.AddAsync(courseDb);
            courseDb.RelatedUsers = new List<ClassroomAdmin>();
            courseDb.RelatedUsers.Add(classroomAdmin);
            await _context.SaveChangesAsync();

            return CreateCourseInfoModelFromCourseDbModel(courseDb);
        }

        public async Task DeleteCourse(string courseId, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            var response = classroomService.Courses.Delete(courseId).Execute();
            CourseDbModel? course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId && c.RelatedUsers.Contains(classroomAdmin));
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CourseInfoModel> PatchCourse(string courseId, CoursePatching parameters, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
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

        public async Task<CourseInfoModel> UpdateCourse(string courseId, CoursePatching parameters, string relatedUser)
        {
            ClassroomService classroomService = _googleClassroomService.GetClassroomService(relatedUser);
            Course course = classroomService.Courses.Get(courseId).Execute();
            if (parameters.Name != null) course.Name = parameters.Name;
            if (parameters.OwnerId != null) course.OwnerId = parameters.OwnerId;
            if (parameters.Section != null) course.Section = parameters.Section;
            if (parameters.Room != null) course.Room = parameters.Room;
            if (parameters.Description != null) course.Description = parameters.Description;
            if (parameters.DescriptionHeading != null) course.DescriptionHeading = parameters.DescriptionHeading;
            if (parameters.CourseState != null) course.CourseState = parameters.CourseState;

            course = classroomService.Courses.Update(course, courseId).Execute();

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
            courseInfoModel.CourseState = course.CourseState;

            return courseInfoModel;
        }

        public async Task<CourseInfoModel> AddCourseIfNotExistsInDb(string courseId, string relatedUser)
        {
            ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == relatedUser);
            if (classroomAdmin == null)
            {
                throw new ArgumentException();
            }
            CourseDbModel? courseDb = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (courseDb == null)
            {
                Course course = GetCourseFromGoogleClassroom(courseId, relatedUser);
                courseDb = new CourseDbModel
                {
                    Id = course.Id,
                    Name = course.Name,
                    Section = course.Section,
                    Description = course.Description,
                    DescriptionHeading = course.DescriptionHeading,
                    Room = course.Room,
                    CourseState = (int)Enum.Parse<CourseStatesEnum>(course.CourseState),
                    HasAllTeachers = true
                };
                
                await _context.Courses.AddAsync(courseDb);
                courseDb.RelatedUsers = new List<ClassroomAdmin>();
                courseDb.RelatedUsers.Add(classroomAdmin);
                await _context.SaveChangesAsync();
            }
            else
            {
                courseDb.RelatedUsers = new List<ClassroomAdmin>();
                if (!courseDb.RelatedUsers.Contains(classroomAdmin))
                {
                    courseDb.RelatedUsers.Add(classroomAdmin);
                    await _context.SaveChangesAsync();
                }
            }

            return CreateCourseInfoModelFromCourseDbModel(courseDb);
        }

        public async Task<List<CourseInfoModel>> SynchronizeCoursesListsInDbAndGoogleClassroom(string relatedUser)
        {
            CourseSearch courseSearch = new CourseSearch();
            List<CourseInfoModel> courses = GetCoursesListFromGoogleClassroom(courseSearch, relatedUser);
            List<CourseInfoModel> response = new List<CourseInfoModel>();
            foreach (var course in courses)
            {
                response.Add(await AddCourseIfNotExistsInDb(course.CourseId, relatedUser));
            }

            return response;
        }

        //private async Task<bool> CheckIfUserRelatedToCourse(string courseId, string user)
        //{
        //    ClassroomAdmin? classroomAdmin = await _context.ClassroomAdmins.FirstOrDefaultAsync(ca => ca.Email == user);
        //    List<CourseDbModel> courses = await _context.Courses
        //        .Where(c => c.Id == courseId).ToListAsync();
        //    foreach (var course in courses)
        //    {
        //        if (course.RelatedUsers.FirstOrDefault(ru => ru == classroomAdmin) != null)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
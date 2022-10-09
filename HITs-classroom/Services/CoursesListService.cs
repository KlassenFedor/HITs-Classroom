using HITs_classroom.Enums;
using HITs_classroom.Jobs;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.CoursesList;
using HITs_classroom.Models.Task;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom.Services
{
    public interface ICoursesListService
    {
        Task<int> CreateCoursesList(List<string> courses);
    }
    public class CoursesListService: ICoursesListService
    {
        private ApplicationDbContext _context;

        public CoursesListService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateCoursesList(List<string> courses)
        {
            AssignedTask task = new AssignedTask
            {
                CreationTime = DateTimeOffset.Now.ToUniversalTime(),
                Status = (int)TaskStatusEnum.NEW
            };
            await _context.Tasks.AddAsync(task);

            foreach (var course in courses)
            {
                CoursePreCreatingModel newCourse = new CoursePreCreatingModel
                {
                    Name = course,
                    Task = task,
                    IsCreated = false,
                    RealCourse = null
                };
                await _context.PreCreatedCourses.AddAsync(newCourse);
            }

            await _context.SaveChangesAsync();

            CoursesScheduler.Start(task.Id);

            return task.Id;
        }

        public async Task<TaskInfoModel> GetTaskInfo(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            TaskInfoModel response = new TaskInfoModel();
            if (task == null)
            {
                response.TaskId = taskId;
                response.Status = TaskStatusEnum.NOT_FOUND.ToString();

                return response;
            }
            response.TaskId = task.Id;
            response.Status = ((TaskStatusEnum)task.Status).ToString();

            switch (task.Status)
            {
                case (int)TaskStatusEnum.NEW:
                    response.CoursesCreated = 0;
                    response.CoursesAssigned = _context.PreCreatedCourses.Where(c => c.Task == task).Count();
                    response.Courses = new List<CourseNameAndIdModel>();
                    break;
                case (int)TaskStatusEnum.IN_PROCESS:
                    response.CoursesCreated = _context.PreCreatedCourses.Where(c => c.Task == task && c.IsCreated).Count();
                    response.CoursesAssigned = _context.PreCreatedCourses.Where(c => c.Task == task).Count();
                    response.Courses = new List<CourseNameAndIdModel>();
                    break;
                case (int)TaskStatusEnum.COMPLETED:
                    List<CoursePreCreatingModel> courses = await _context.PreCreatedCourses.Where(c => c.Task == task).ToListAsync();

                    response.CoursesCreated = courses.Where(c => c.IsCreated && c.RealCourse != null).Count();
                    response.CoursesAssigned = courses.Count();
                    response.Courses = new List<CourseNameAndIdModel>();
                    foreach (var course in courses)
                    {
                        var newResponseCourse = new CourseNameAndIdModel();
                        newResponseCourse.CourseName = course.Name;
                        if (course.IsCreated && course.RealCourse != null)
                        {
                            newResponseCourse.CourseId = course.RealCourse.Id;
                        }
                        response.Courses.Add(newResponseCourse);
                    }
                    break;
                default:
                    break;
            }

            return response;
        }
    }
}

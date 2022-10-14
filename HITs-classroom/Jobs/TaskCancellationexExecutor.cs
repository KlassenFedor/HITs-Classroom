using Google.Apis.Classroom.v1;
using Google.Apis.Classroom.v1.Data;
using HITs_classroom.Models.Course;
using HITs_classroom.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using static Google.Apis.Classroom.v1.CoursesResource.ListRequest;

namespace HITs_classroom.Jobs
{
    public class TaskCancellationexExecutor: IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connection = configuration.GetConnectionString("DefaultConnection");

            var serviceProvider = new ServiceCollection()
                .AddScoped<ICoursesService, CoursesService>()
                .AddScoped<GoogleClassroomServiceForServiceAccount>()
                .AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connection))
                .BuildServiceProvider();

            var schedulerContext = context.Scheduler.Context;
            try
            {
                var classroomService = serviceProvider.GetRequiredService<GoogleClassroomServiceForServiceAccount>().GetClassroomService();

                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == (int)schedulerContext.Get("task"));

                if (task != null)
                {
                    var createdCourses = await dbContext.PreCreatedCourses
                        .Where(c => c.Task == task && c.RealCourse != null).Include(c => c.RealCourse).ToListAsync();
                    foreach (var course in createdCourses)
                    {
                        if (course.RealCourse != null)
                        {
                            await DeleteCourse(course.RealCourse.Id, dbContext, classroomService);
                        }
                    }
                    foreach (var preCreatedCourse in
                        await dbContext.PreCreatedCourses.Where(c => c.Task == task).ToListAsync())
                    {
                        dbContext.Remove(preCreatedCourse);
                    }
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    ILogger<TaskCancellationexExecutor> logger =
                        serviceProvider.GetRequiredService<ILogger<TaskCancellationexExecutor>>();
                    logger.LogError("Task with id={id} doesn't found.", (int)schedulerContext.Get("task"));
                }
            }
            catch (Exception e)
            {
                ILogger<TaskCancellationexExecutor> logger =
                    serviceProvider.GetRequiredService<ILogger<TaskCancellationexExecutor>>();
                logger.LogError("Error during task cancellation with taskId={id}. Error: {error}",
                    (int)schedulerContext.Get("task"), e.Message);
            }
        }

        private async Task DeleteCourse(string courseId, ApplicationDbContext context, ClassroomService service)
        {
            await ArchiveCourse(courseId, context, service);
            var response = await service.Courses.Delete(courseId).ExecuteAsync();
            CourseDbModel? course = await context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course != null)
            {
                context.Courses.Remove(course);
                await context.SaveChangesAsync();
            }
        }

        private async Task ArchiveCourse(string courseId, ApplicationDbContext context, ClassroomService service)
        {
            string updateMask = "courseState";

            Course course = new Course
            {
                CourseState = "ARCHIVED"
            };
            var request = service.Courses.Patch(course, courseId);
            request.UpdateMask = updateMask;
            course = await request.ExecuteAsync();

            CourseDbModel? courseDb = await context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (courseDb != null)
            {
                courseDb.CourseState = (int)Enum.Parse<CourseStatesEnum>("ARCHIVED");
                await context.SaveChangesAsync();
                return;
            }

            throw new NullReferenceException();
        }
    }
}

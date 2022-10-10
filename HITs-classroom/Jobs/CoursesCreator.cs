using HITs_classroom.Enums;
using HITs_classroom.Models.Course;
using HITs_classroom.Models.CoursesList;
using HITs_classroom.Models.Task;
using HITs_classroom.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace HITs_classroom.Jobs
{
    public class CoursesCreator: IJob
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
                var coursesService = serviceProvider.GetRequiredService<ICoursesService>();

                var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == (int)schedulerContext.Get("task"));

                if (task != null)
                {
                    task.Status = (int)TaskStatusEnum.IN_PROCESS;
                    await dbContext.SaveChangesAsync();
                    await CreateCoursesList(dbContext, task, coursesService);
                }
                else
                {
                    ILogger<CoursesCreator> logger = serviceProvider.GetRequiredService<ILogger<CoursesCreator>>();
                    logger.LogError("Task with id={id} doesn't found.", (int)schedulerContext.Get("task"));
                }
            }
            catch (Exception e)
            {
                ILogger<CoursesCreator> logger = serviceProvider.GetRequiredService<ILogger<CoursesCreator>>();
                logger.LogError("Error during courses creating for task with id={id}. Error: {error}",
                    (int)schedulerContext.Get("task"), e.Message);
            }
        }

        private async Task CreateCourse(
            ApplicationDbContext dbContext,
            AssignedTask task,
            ICoursesService coursesService,
            CoursePreCreatingModel preCreatedCourse)
        {
            CourseShortModel courseCreatingModel = new CourseShortModel
            {
                Name = preCreatedCourse.Name,
                Section = preCreatedCourse.Section,
                Room = preCreatedCourse.Room,
                Description = preCreatedCourse.Description,
                DescriptionHeading = preCreatedCourse.DescriptionHeading,
                OwnerId = "me"
            };
            var newCourse = await coursesService.CreateCourse(courseCreatingModel);
            var realCourse = await dbContext.Courses.FirstOrDefaultAsync(c => c.Id == newCourse.CourseId);

            if (realCourse != null)
            {
                preCreatedCourse.IsCreated = true;
                preCreatedCourse.RealCourse = realCourse;
                await dbContext.SaveChangesAsync();
            }
        }

        private async Task CreateCoursesList(
                ApplicationDbContext dbContext,
                AssignedTask task,
                ICoursesService coursesService)
        {
            var preCreatedCourses = await dbContext.PreCreatedCourses.Where(c => c.TaskId == task.Id && !c.IsCreated).ToListAsync();
            try
            {
                foreach (var preCreatedCourse in preCreatedCourses)
                {
                    await CreateCourse(dbContext, task, coursesService, preCreatedCourse);
                }
                task.Status = (int)TaskStatusEnum.COMPLETED;
                task.EndTime = DateTimeOffset.Now.ToUniversalTime();
                await dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                task.Status = (int)TaskStatusEnum.FAILED;
                await dbContext.SaveChangesAsync();
                ILoggerFactory loggerFactory = new LoggerFactory();
                ILogger<CoursesCreator> logger = new Logger<CoursesCreator>(loggerFactory);
                logger.LogError(e.Message);
            }
        }
    }
}

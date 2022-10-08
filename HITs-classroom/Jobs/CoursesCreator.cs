using HITs_classroom.Models.Course;
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
            var coursesService = serviceProvider.GetRequiredService<ICoursesService>();

            var courses = (List<CourseShortModel>)schedulerContext.Get("courses");
            foreach (var course in courses)
            {
                await coursesService.CreateCourse(course);
            }
        }
    }
}

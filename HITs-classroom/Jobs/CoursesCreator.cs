using HITs_classroom.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Claims;

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
                .AddScoped<GoogleClassroomService>()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection))
                .BuildServiceProvider();

            var coursesService = serviceProvider.GetRequiredService<ICoursesService>();
        }
    }
}

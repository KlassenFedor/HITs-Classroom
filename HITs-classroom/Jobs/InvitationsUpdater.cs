using Quartz;
using HITs_classroom.Services;
using Microsoft.EntityFrameworkCore;

namespace HITs_classroom.Jobs
{
    public class InvitationsUpdater: IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();
            string connection = configuration.GetConnectionString("DefaultConnection");

            var serviceProvider = new ServiceCollection()
                .AddScoped<IInvitationsService, InvitationsService>()
                .AddScoped<GoogleClassroomService>()
                .AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection))
                .BuildServiceProvider();

            var schedulerContext = context.Scheduler.Context;
            var invitationsService = serviceProvider.GetRequiredService<IInvitationsService>();

            var relatedUser = (string)schedulerContext.Get("user");
            await invitationsService.UpdateAllInvitations(relatedUser);
        }
    }
}

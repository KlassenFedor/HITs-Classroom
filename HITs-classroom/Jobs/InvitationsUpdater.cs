using Quartz;
using System.Net.Mail;
using System.Net;
using HITs_classroom.Services;
using Google.Apis.Classroom.v1;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

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

            var invitationsService = serviceProvider.GetRequiredService<IInvitationsService>();
            //await invitationsService.UpdateAllInvitations();
        }
    }
}

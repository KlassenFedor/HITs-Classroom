using Quartz.Impl;
using Quartz;
using System.Diagnostics;
using Google.Apis.Classroom.v1.Data;

namespace HITs_classroom.Jobs
{
    public class InvitationsScheduler
    {
        public static async void Start(string relatedUser)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.UnscheduleJob(new TriggerKey("invitationsUpdating", relatedUser));
            await scheduler.Start();
            scheduler.Context.Put("user", relatedUser);

            IJobDetail job = JobBuilder.Create<InvitationsUpdater>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("invitationsUpdating", relatedUser)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

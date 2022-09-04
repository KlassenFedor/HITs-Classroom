using Quartz.Impl;
using Quartz;
using System.Diagnostics;

namespace HITs_classroom.Jobs
{
    public class InvitationsScheduler
    {
        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<InvitationsUpdater>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("invitationsUpdating", "ivitationsGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(86400)
                    .RepeatForever())
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

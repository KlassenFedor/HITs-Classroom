using Quartz.Impl;
using Quartz;
using System.Diagnostics;
using Google.Apis.Classroom.v1.Data;

namespace HITs_classroom.Jobs
{
    public class InvitationsScheduler
    {
        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.UnscheduleJob(new TriggerKey("invitationsUpdating", "classroomServcie"));
            await scheduler.Start();

            IJobDetail job = JobBuilder.Create<InvitationsUpdater>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("invitationsUpdating", "classroomServcie")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

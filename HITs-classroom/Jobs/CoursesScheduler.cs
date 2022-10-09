using Quartz.Impl;
using Quartz;

namespace HITs_classroom.Jobs
{
    public class CoursesScheduler
    {
        public static async void Start(int taskId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.UnscheduleJob(new TriggerKey("coursesCreating", "classroomServcie"));
            await scheduler.Start();
            scheduler.Context.Put("task", taskId);

            IJobDetail job = JobBuilder.Create<CoursesCreator>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("coursesCreating", "classroomServcie")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();
 
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

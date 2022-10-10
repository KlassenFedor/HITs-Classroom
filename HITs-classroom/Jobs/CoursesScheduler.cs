using Quartz.Impl;
using Quartz;

namespace HITs_classroom.Jobs
{
    public class CoursesScheduler
    {
        public static async void Start(int taskId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            scheduler.Context.Put("task", taskId);

            IJobDetail job = JobBuilder.Create<CoursesCreator>().WithIdentity("jobKey_" + taskId.ToString()).Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerKey_" + taskId.ToString(), "coursesCreating")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();
 
            await scheduler.ScheduleJob(job, trigger);
        }

        public static async void Stop(int taskId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.PauseJob(new JobKey("jobKey_" + taskId.ToString(), "coursesCreating"));
            await scheduler.UnscheduleJob(new TriggerKey("triggerKey_" + taskId.ToString(), "coursesCreating"));
        }
    }
}

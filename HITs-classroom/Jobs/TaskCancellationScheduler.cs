using Quartz.Impl;
using Quartz;

namespace HITs_classroom.Jobs
{
    public static class TaskCancellationScheduler
    {
        public static async void Start(int taskId)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            scheduler.Context.Put("task", taskId);

            IJobDetail job = JobBuilder.Create<TaskCancellationexExecutor>()
                .WithIdentity("jobKeyCancellation_" + taskId.ToString()).Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("triggerKeyCancellation_" + taskId.ToString(), "coursesCancellation")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

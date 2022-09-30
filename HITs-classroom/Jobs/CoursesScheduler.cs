using Quartz.Impl;
using Quartz;
using HITs_classroom.Models.Course;
using System.Diagnostics;

namespace HITs_classroom.Jobs
{
    public class CoursesScheduler
    {
        public static async void Start(string relatedUser, List<CourseShortModel> courses)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.UnscheduleJob(new TriggerKey("coursesCreating", relatedUser));
            await scheduler.Start();
            scheduler.Context.Put("courses", courses);
            scheduler.Context.Put("user", relatedUser);

            IJobDetail job = JobBuilder.Create<CoursesCreator>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("coursesCreating", relatedUser)
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(0))
                .Build();
 
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}

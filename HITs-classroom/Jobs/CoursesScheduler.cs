using Quartz.Impl;
using Quartz;

namespace HITs_classroom.Jobs
{
    public class CoursesScheduler
    {
        public static async void Start(string relatedUser)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            List<int> newList = new List<int>();
            scheduler.Context.Put("key", newList);

            IJobDetail job = JobBuilder.Create<CoursesCreator>().Build();
        }
    }
}

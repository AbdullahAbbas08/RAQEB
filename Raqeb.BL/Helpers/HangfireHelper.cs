using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using System.Linq.Expressions;

namespace Raqeb.Helper
{
    public static class HangfireHelper
    {
        public static string CreateBackgroundJob(this IBackgroundJobClient backgroundJobClient, JobQueues jobQueue, Expression<Func<Task>> methodCall)
        {
            EnqueuedState state = new(jobQueue.ToString().ToLower());
            return backgroundJobClient.Create(methodCall, state);
        }

        public static bool IsAnyActiveJobs(JobQueues jobQueue)
        {
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
            long enqueued = monitoringApi.EnqueuedCount(jobQueue.ToString().ToLower());
            return monitoringApi.Queues().FirstOrDefault(q => q.Name == jobQueue.ToString().ToLower()) != null;
        }

        public static bool JobExist(JobQueues jobQueue, string jobId)
        {
            IMonitoringApi monitoringApi = JobStorage.Current.GetMonitoringApi();
            QueueWithTopEnqueuedJobsDto queue = monitoringApi.Queues().FirstOrDefault(q => q.Name == jobQueue.ToString().ToLower());
            if (queue != null)
            {
                bool jobExist = queue.FirstJobs.Any(p => p.Key == jobId);
                return jobExist;
            }
            return false;
        }
        public static void WaitJob(this IBackgroundJobClient backgroundJobClient, JobQueues jobQueue, string jobId)
        {
            while (JobExist(jobQueue, jobId)) { }
        }

        public static string[] GetQueues()
        {
            return Enum.GetNames(typeof(JobQueues));
        }
    }

    public enum JobQueues
    {
        Default,
        SendEmail,
        UploadAzureRIPricelist,
        CartUniqeID,
        OrderUniqeId,
        STSBilling,
        MicrosoftSubscriptionSuspend
    }

}

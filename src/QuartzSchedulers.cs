using System.Collections.Specialized;

namespace voddy {
    public class QuartzSchedulers {
        public static NameValueCollection PrimaryScheduler() {
            NameValueCollection nameValueCollection = new NameValueCollection {
                { "quartz.scheduler.instanceName", "PrimaryScheduler" },
                { "quartz.scheduler.instanceId", "PrimaryScheduler" },
                { "quartz.jobStore.type", "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz" }
            };

            return nameValueCollection;
        }

        public static NameValueCollection SingleThreadScheduler() {
            NameValueCollection nameValueCollection = new NameValueCollection {
                { "quartz.scheduler.instanceName", "SingleThreadScheduler" },
                { "quartz.scheduler.instanceId", "SingleThreadScheduler"},
                { "quartz.threadPool.maxConcurrency", "1" }
            };

            return nameValueCollection;
        }

        public static NameValueCollection RamScheduler() {
            NameValueCollection nameValueCollection = new NameValueCollection {
                { "quartz.scheduler.instanceName", "RAMScheduler" },
                { "quartz.scheduler.instanceId", "RAMScheduler" }
            };

            return nameValueCollection;
        }
    }
}
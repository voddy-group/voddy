using System;
using System.Collections;
using System.Linq;
using voddy.Databases.Logs;
using voddy.Databases.Main;

namespace voddy.Controllers.BackgroundTasks {
    public class CleanUpDatabase {
        public void TrimLogs() {
            using (var context = new LogDataContext()) {
                var records = context.Logs.AsEnumerable().OrderByDescending(item => DateTime.Parse(item.logged))
                    .Skip(7500);
                foreach (var log in records) {
                    context.Remove(log);
                }

                context.SaveChanges();
            }
        }
    }
}
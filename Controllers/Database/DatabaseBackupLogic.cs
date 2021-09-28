using System;
using System.IO;
using System.Linq;
using System.Threading;
using Hangfire;
using Microsoft.Data.Sqlite;
using NLog;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.Database {
    public class DatabaseBackupLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();
        
        [DisableConcurrentExecution(10)]
        public void BackupChatDatabase() {
            while (true) {
                bool isChatDownloading;
                using (var context = new MainDataContext()) {
                    isChatDownloading = context.Streams.Any(item => item.chatDownloading);
                }

                if (isChatDownloading) {
                    _logger.Warn("Chat downloading! Will wait a minute.");
                    Thread.Sleep(60000);
                } else {
                    BackupDatabase("chatDb");
                    return;
                }
            }
        }
        
        [DisableConcurrentExecution(10)]
        public void BackupDatabase(string sourceDbName) {

            var source = new SqliteConnection(@"Data Source=/storage/voddy/databases/" + sourceDbName + ".db");
            DirectoryInfo backupFolder = new DirectoryInfo(GlobalConfig.GetGlobalConfig("contentRootPath") + "databases/backup/" + sourceDbName);
            if (!backupFolder.Exists) {
                backupFolder.Create();
            }

            string backupFilePath = Path.Combine(backupFolder.FullName,
                sourceDbName + $"-backup-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}") + ".db";
            var backup = new SqliteConnection(
                @"Data Source=" + backupFilePath);
            backup.Open();
            source.Open();
            source.BackupDatabase(backup);
            using (var context = new MainDataContext()) {
                var dbBackup = new Backup {
                    type = sourceDbName,
                    datetime = DateTime.UtcNow,
                    location = backupFilePath
                };

                context.Add(dbBackup);
                context.SaveChanges();
            }
        }
    }
}
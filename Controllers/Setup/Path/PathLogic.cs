using System;
using System.IO;
using System.Linq;
using System.Text;
using Hangfire;
using voddy.Data;

namespace voddy.Controllers.Setup.Path {
    public class PathLogic {
        public bool UpdatePathLogic(UpdatePathJson data) {
            /*Migration migration = CheckIfMigrationRunningLogic();

            if (migration.running) {
                return false;
            }*/

            string newPath = SanatizeNewPath(data.NewPath);

            if (!CheckWriteAccess(newPath)) {
                return false;
            }

            //if (data.AlreadyMoved) {
            UpdateDatabase(newPath);
            return true;
            //}

            /*string contentRootPath;
            using (var context = new DataContext()) {
                contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;
            }

            BackgroundJob.Enqueue<PathLogic>(item => item.CopyJob(contentRootPath, newPath));*/
            //return true;
        }

        public bool CheckWriteAccess(string newPath) {
            DirectoryInfo newPathDirectory = new DirectoryInfo(newPath);
            try {
                File.Create(newPathDirectory.FullName + "test.txt");
            } catch (Exception) {
                return false;
            }

            File.Delete(newPathDirectory.FullName + "test.txt");
            return true;
        }

        public string SanatizeNewPath(string newPath) {
            string returnPath = newPath;
            if (!newPath.EndsWith("/")) {
                returnPath = newPath + "/";
            }

            return returnPath;
        }

        public void UpdateDatabase(string newPath) {
            using (var context = new DataContext()) {
                var currentContentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath");
                currentContentRootPath.value = newPath;
                context.SaveChanges();
            }
        }
        /*[JobDisplayName("Copy")]
        public void CopyJob(string contentRootPath, string newPath) {
            if (CopyFolder(contentRootPath, newPath)) {
                UpdateDatabase(newPath);
                DirectoryInfo oldPath = new DirectoryInfo(contentRootPath);
                // delete old path
                oldPath.Delete(true);
            }
        }

        public bool CopyFolder(string source, string destination) {
            DirectoryInfo currentPath = new DirectoryInfo(source);

            DirectoryInfo[] subDirs = currentPath.GetDirectories();

            DirectoryInfo newFolder = new DirectoryInfo(destination);
            try {
                newFolder.Create();
            } catch (IOException) {
                return false;
            }

            CopyFiles(currentPath, newFolder);
            CopySubDirs(subDirs, newFolder);
            return true;
        }

        public void CopyFiles(DirectoryInfo source, DirectoryInfo destination) {
            FileInfo[] files = source.GetFiles();
            foreach (var file in files) {
                string filePath = System.IO.Path.Combine(destination.FullName, file.Name);
                file.CopyTo(filePath);
            }
        }

        public void CopySubDirs(DirectoryInfo[] source, DirectoryInfo destination) {
            foreach (var subDir in source) {
                string dirPath = System.IO.Path.Combine(destination.FullName, subDir.Name);
                CopyFolder(subDir.FullName, dirPath);
            }
        }

        public Migration CheckIfMigrationRunningLogic() {
            var runningJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, 100);
            for (var x = 0; x < runningJobs.Count; x++) {
                if (runningJobs[x].Value.Job.Method.Name == "CopyJob") {
                    return new Migration {
                        running = true
                    };
                }
            }

            return new Migration {
                running = false
            };
        }*/

        public RootPathJson GetCurrentPathLogic() {
            using (var context = new DataContext()) {
                return new RootPathJson {
                    Path = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value
                };
            }
        }
    }

    /*public class Migration {
        public bool running { get; set; }
    }*/
}
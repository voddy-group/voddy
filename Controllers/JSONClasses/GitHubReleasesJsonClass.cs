using System.Collections.Generic;

namespace voddy.Controllers.JSONClasses {
    public class GitHubReleasesJsonClass {
        public class GitHubReleasesRoot
        {
            public string tag_name { get; set; }
            public bool draft { get; set; }
            public bool prerelease { get; set; }
        }
    }
}
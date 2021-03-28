using System;
using System.Collections.Generic;

namespace voddy.Controllers.Structures {
    public class FollowListJsonClass {
        public class FollowListData
        {
            public string from_id { get; set; }
            public string from_login { get; set; }
            public string from_name { get; set; }
            public string to_id { get; set; }
            public string to_login { get; set; }
            public string to_name { get; set; }
            public DateTime followed_at { get; set; }
        }

        public class FollowList
        {
            public List<FollowListData> data { get; set; }
        }
        
        
    }
}
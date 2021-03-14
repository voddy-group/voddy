using System;
using System.Collections.Generic;

namespace voddy.Controllers.Structures {
    public class ChatMessageJsonClass {
        public class Commenter
        {
            public string display_name { get; set; }
            public string _id { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public object bio { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
        }

        public class Emoticons
        {
            public string _id { get; set; }
            public int begin { get; set; }
            public int end { get; set; }
        }

        public class Emoticon
        {
            public string emoticon_id { get; set; }
            public string emoticon_set_id { get; set; }
        }

        public class Fragment
        {
            public string text { get; set; }
            public Emoticon emoticon { get; set; }
        }

        public class UserBadge
        {
            public string _id { get; set; }
            public string version { get; set; }
        }

        public class Message
        {
            public string body { get; set; }
            public List<Emoticons> emoticons { get; set; }
            public List<Fragment> fragments { get; set; }
            public string user_color { get; set; }
            public List<UserBadge> user_badges { get; set; }
        }

        public class Comment
        {
            public string _id { get; set; }
            public DateTime created_at { get; set; }
            public DateTime updated_at { get; set; }
            public double content_offset_seconds { get; set; }
            public Commenter commenter { get; set; }
            public string source { get; set; }
            public Message message { get; set; }
        }

        public class ChatMessage
        {
            public List<Comment> comments { get; set; }
            public string _next { get; set; }
        }
    }
}
using System.Collections.Generic;

namespace voddy.Controllers.Structures {
    public class YoutubeDlVideoJson {
        public class Format    {
            public string format_id { get; set; } 
            public double? tbr { get; set; }
            public string url { get; set; } 
            public string ext { get; set; } 
            public double? fps { get; set; } 
            public int width { get; set; } 
            public int height { get; set; } 
        }

        public class YoutubeDlVideo {
            public int duration { get; set; }
            public List<Format> formats { get; set; } 
            public string format_id { get; set; } 
            public string url { get; set; } 
            public string ext { get; set; } 
            public double? fps { get; set; } 
            public string protocol { get; set; } 
            public object preference { get; set; } 
            public int width { get; set; } 
            public int height { get; set; } 
            public string format { get; set; } 
            public string chosenResolution { get; set; }
            public string _filename { get; set; }
        }

        public class YoutubeDlVideoInfo {
            public int quality { get; set; }
            public int duration { get; set; }
            public string url { get; set; }
            public string filename { get; set; }
            public string formatId { get; set; }
        }
    }
}
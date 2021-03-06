using System;
using System.IO;
using System.Net.Http;

namespace voddy {
    public class DownloadHelpers {
        
        public static void DownloadFile(string url, string location) {
            HttpClient client = new HttpClient();
            var contentBytes = client.GetByteArrayAsync(new Uri(url)).Result;
            MemoryStream stream = new MemoryStream(contentBytes);
            FileStream file = new FileStream(location, FileMode.Create, FileAccess.Write);
            stream.WriteTo(file);
            file.Close();
            stream.Close();
        }
    }
}
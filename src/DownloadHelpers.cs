using System.Collections.Generic;
using System.IO;
using RestSharp;

namespace voddy {
    public class DownloadHelpers {
        
        public string DownloadFile(string url, string location) {
            var client = new RestClient(url);
            var response = client.Execute(new RestRequest(""));
            string returnValue = "";
                for (var x = 0; x < response.Headers.Count; x++) {
                    if (response.Headers[x].Name == "ETag") {
                        var etag = response.Headers[x].Value;
                        if (etag != null) {
                            returnValue = etag.ToString().Replace("\"", "");
                        }
                    }
                }
            File.WriteAllBytes(location, response.RawBytes);
            return returnValue;
        }

        public IList<Parameter> GetHeaders(string url) {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(Method.HEAD);
            IRestResponse response = client.Execute(request);
            return response.Headers;
        }
    }
}
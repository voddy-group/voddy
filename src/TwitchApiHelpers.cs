using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using voddy.Controllers;
using voddy.Controllers.Structures;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy {
    public class TwitchApiHelpers {
        //private DataContext _dataContext;
        private Authentication _authentication;
        private string url { get; set; }
        private Method method { get; set; }

        private int allowedRetries = 3;

        private Logger _logger { get; set; } = new NLog.LogFactory().GetCurrentClassLogger();

        public TwitchApiHelpers() {
            using (var context = new MainDataContext()) {
                _authentication = context.Authentications.FirstOrDefault(item => item.service == "twitch");
            }
        }


        public IRestResponse TwitchRequest(string url, Method method) {
            this.url = url;
            this.method = method;

            return ValidTokenCheck(url, method);
        }

        public IRestResponse LegacyTwitchRequest(string url, Method method) {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("Client-Id", _authentication.clientId);
            request.AddHeader("Accept", "application/vnd.twitchtv.v5+json; charset=UTF-8");
            return RequestExecutionExceptionHandler(client, request);
        }

        private IRestResponse ValidTokenCheck(string url, Method method) {
            IRestResponse response = Request(url, method);

            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                // access token expired
                var client = new RestClient("https://id.twitch.tv/oauth2/token" +
                                            "?grant_type=refresh_token" +
                                            $"&refresh_token={_authentication.refreshToken}" +
                                            $"&client_id={_authentication.clientId}" +
                                            $"&client_secret={_authentication.clientSecret}");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                IRestResponse reAuth = client.Execute(request); // generate a new one

                if (reAuth.IsSuccessful) {
                    var reAuthResponse = JsonConvert.DeserializeObject<RefreshToken>(reAuth.Content);

                    using (var context = new MainDataContext()) {
                        _authentication = new Authentication();
                        _authentication = context.Authentications.FirstOrDefault(item => item.service == "twitch");

                        _authentication.accessToken = reAuthResponse.access_token;
                        _authentication.refreshToken = reAuthResponse.refresh_token;

                        context.SaveChanges(); // save new credentials to db
                    }

                    return Request(url, method); // run original request again using new credentials
                } else {
                    return response;
                    // refresh token not working; todo HANDLE THIS. Likely user will have to re-auth.
                }
            }

            if (response.IsSuccessful) {
                return response;
            }

            // is error
            _logger.Error(response.StatusCode + ":" + response.ErrorMessage);

            return response;
        }

        private IRestResponse Request(string url, Method method) {
            var client = new RestClient(url);
            client.Timeout = -1;
            var request = new RestRequest(method);
            request.AddHeader("client-id", _authentication.clientId);
            request.AddHeader("Authorization", $"Bearer {_authentication.accessToken}");
            var returnValue = RequestExecutionExceptionHandler(client, request);

            // set connection error to false if the record exists in the db
            var config = GlobalConfig.GetGlobalConfig("connectionError");
            if (config is "True") {
                GlobalConfig.SetGlobalConfig("connectionError", false.ToString());
            }

            NotificationHub.Current.Clients.All.SendAsync("noConnection", "false");

            return returnValue;
        }

        private IRestResponse RequestExecutionExceptionHandler(RestClient client, RestRequest request) {
            IRestResponse returnValue = null;
            int currentRetries = 0;
            var sleepDuration = 2000;

            while (currentRetries < allowedRetries) {
                returnValue = client.Execute(request);
                if (returnValue.StatusCode != HttpStatusCode.OK && returnValue.StatusCode != HttpStatusCode.Unauthorized) {
                    // unauthorized is probably access token expired, can be handled by above token check when returned
                    switch (currentRetries) {
                        case 1:
                            sleepDuration = 10000;
                            break;
                        case 2:
                            sleepDuration = 20000;
                            break;
                    }

                    currentRetries++;
                    Console.WriteLine($"Encountered error, will retry in {sleepDuration / 1000} seconds...");
                    Thread.Sleep(sleepDuration);
                    continue;
                }

                break;
            }

            if (currentRetries == allowedRetries) {
                _logger.Error($"Could not recover. URL: '{url}'; status code: '{returnValue.StatusCode}'");

                switch (returnValue.StatusCode) {
                    case 0:
                        // cannot even start connection, must be disconnection
                        GlobalConfig.SetGlobalConfig("connectionError", true.ToString());

                        NotificationHub.Current.Clients.All.SendAsync("noConnection", "true");
                        throw new NetworkInformationException(0);
                        break;
                    case HttpStatusCode.NotFound:
                        throw new NetworkInformationException(404);
                }
            }

            return returnValue;
        }

        public class RefreshToken {
            public string access_token { get; set; }
            public string refresh_token { get; set; }
        }
    }
}
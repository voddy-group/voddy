using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using voddy.Controllers.BackgroundTasks;
using voddy.Controllers.Setup.TwitchAuthentication;
using voddy.Databases.Chat;
using voddy.Databases.Chat.Models;
using voddy.Databases.Main;

namespace voddy.Controllers.LiveStreams {
    public class LiveStreamChatLogic {
        private Logger _logger { get; set; } = NLog.LogManager.GetCurrentClassLogger();

        public Task DownloadLiveStreamChatLogic(string channel, long vodId, CancellationToken cancellationToken) {
            using (var irc = new TcpClient("irc.chat.twitch.tv", 6667))
            using (var stream = irc.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream)) {
                writer.AutoFlush = true;
                string accessToken;

                using (var context = new MainDataContext()) {
                    accessToken = context.Authentications.First().accessToken;
                    //todo check for working auth on all external calls
                }
                var dbUserName = GlobalConfig.GetGlobalConfig("userName");

                var userName = dbUserName ?? new UserDetails().SaveUserDataToDb();

                writer.WriteLine($"PASS oauth:{accessToken}");
                writer.WriteLine($"NICK {userName}");
                writer.WriteLine($"JOIN #{channel}");
                writer.WriteLine("CAP REQ :twitch.tv/tags");

                string inputLine;
                int databaseCounter = 0;
                List<Chat> chats = new List<Chat>();
                while ((inputLine = reader.ReadLine()) != null) {
                    if (cancellationToken.IsCancellationRequested) {
                        _logger.Warn("IRC shut down initiated, stream must have finished...");
                        AddLiveStreamChatToDb(chats, vodId);
                        StreamHelpers.SetChatDownloadToFinished(vodId, true);
                        _logger.Info("Done!");
                        break;
                    }

                    if (inputLine == "PING :tmi.twitch.tv") {
                        writer.WriteLine("PONG :tmi.twitch.tv");
                    }

                    if (inputLine.Contains("PRIVMSG")) {
                        Chat chat = MessageBuilder(inputLine);
                        chat.streamId = vodId;
                        chats.Add(chat);
                        databaseCounter++;
                    }

                    if (databaseCounter == 50) {
                        AddLiveStreamChatToDb(chats, vodId);
                        databaseCounter = 0;
                        chats.Clear();
                    }
                }

                // todo check emote compatibility; does it send offline notification in irc??
            }

            return Task.CompletedTask;
        }

        public static Chat MessageBuilder(string input) {
            Chat message = new Chat();
            message.sentAt = DateTime.Now;

            var splitInput = input.Split(";");
            foreach (var messageDetails in splitInput) {
                Dictionary<string, string> messageInfo = new Dictionary<string, string>();
                var furtherSplitting = messageDetails.Split("=");
                if (furtherSplitting.Length > 1) {
                    messageInfo.Add(furtherSplitting[0], furtherSplitting[1].Split(" ")[0]);

                    if (messageInfo.ContainsKey("badges")) {
                        if (messageInfo["badges"].Length > 0) {
                            // if user has badges
                            message.userBadges = messageInfo["badges"].Replace("/", ":");
                        }

                        message.userBadges = messageInfo["badges"];
                    }

                    if (messageInfo.ContainsKey("color") && messageInfo["color"] != "") {
                        message.userColour = messageInfo["color"];
                    }

                    if (messageInfo.ContainsKey("display-name")) {
                        message.userName = messageInfo["display-name"];
                    }

                    if (messageInfo.ContainsKey("user-id")) {
                        message.userId = messageInfo["user-id"];
                    }

                    if (messageInfo.ContainsKey("id")) {
                        message.messageId = messageInfo["id"];
                    }

                    if (messageInfo.ContainsKey("mod")) {
                        message.mod = int.Parse(messageInfo["mod"]) == 1;
                    }

                    if (messageInfo.ContainsKey("subscriber")) {
                        message.subscriber = int.Parse(messageInfo["subscriber"]) == 1;
                    }

                    if (messageInfo.ContainsKey("turbo")) {
                        message.turbo = int.Parse(messageInfo["turbo"]) == 1;
                    }

                    if (messageInfo.ContainsKey("emotes")) {
                        message.emotes = messageInfo["emotes"];
                    }

                    if (messageInfo.ContainsKey("tmi-sent-ts")) {
                        DateTime messageSentDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                        messageSentDateTime = messageSentDateTime.AddMilliseconds(long.Parse(messageInfo["tmi-sent-ts"]));
                        message.sentAt = messageSentDateTime;
                    }

                    int start = input.LastIndexOf(":") + 1;
                    message.body = input.Substring(start, input.Length - start);
                }
            }

            return message;
        }

        public void AddLiveStreamChatToDb(List<Chat> chats, long streamId) {
            using (var context = new ChatDataContext()) {
                _logger.Info("Saving live chat...");
                for (int i = 0; i < chats.Count; i++) {
                    context.Chats.Add(chats[i]);
                }

                context.SaveChanges();
            }
        }
    }
}
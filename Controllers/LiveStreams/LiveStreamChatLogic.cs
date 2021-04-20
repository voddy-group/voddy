using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Hangfire;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.LiveStreams {
    public class LiveStreamChatLogic {
        [Queue("default")]
        public void DownloadLiveStreamChatLogic(string channel, long vodId, CancellationToken token) {
            using (var irc = new TcpClient("irc.chat.twitch.tv", 6667))
            using (var stream = irc.GetStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream)) {
                writer.AutoFlush = true;
                string accessToken;
                string userName;

                using (var context = new DataContext()) {
                    accessToken = context.Authentications.First().accessToken;
                    //todo check for working auth on all external calls
                    while (true) {
                        var dbUsername = context.Configs.FirstOrDefault(item => item.key == "userName").value;

                        if (dbUsername == null) {
                            UserDetails userDetails = new UserDetails();
                            userDetails.SaveUserDataToDb();
                        } else {
                            userName = dbUsername;
                            break;
                        }
                    }
                }

                writer.WriteLine($"PASS oauth:{accessToken}");
                writer.WriteLine($"NICK {userName}");
                writer.WriteLine($"JOIN #{channel}");
                writer.WriteLine("CAP REQ :twitch.tv/tags");

                string inputLine;
                int databaseCounter = 0;
                List<Chat> chats = new List<Chat>();
                while ((inputLine = reader.ReadLine()) != null) {
                    if (token.IsCancellationRequested) {
                        Console.WriteLine("IRC shut down initiated, stream must have finished...");
                        AddLiveStreamChatToDb(chats, vodId);
                        HandleDownloadStreamsLogic handleDownloadStreamsLogic = new HandleDownloadStreamsLogic();
                        handleDownloadStreamsLogic.SetChatDownloadToFinished(vodId, true);
                        Console.WriteLine("Done!");
                        break;
                    }

                    if (inputLine == "PING :tmi.twitch.tv") {
                        writer.WriteLine("PONG :tmi.twitch.tv");
                    }

                    if (inputLine.Contains("PRIVMSG")) {
                        Chat chat = MessageBuilder(inputLine);
                        chat.streamId = vodId;
                        Console.WriteLine($"{chat.userName}: {chat.body}");
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

                    int start = input.LastIndexOf(":") + 1;
                    message.body = input.Substring(start, input.Length - start);
                }
            }

            return message;
        }

        public void AddLiveStreamChatToDb(List<Chat> chats, long streamId) {
            using (var context = new DataContext()) {
                Console.WriteLine("Saving live chat...");
                for (int i = 0; i < chats.Count; i++) {
                    context.Chats.Add(chats[i]);
                }

                context.SaveChanges();
            }
        }
    }
}
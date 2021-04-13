using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using voddy.Data;

namespace voddy.Controllers.LiveStreams {
    public class LiveStreamChatLogic {
        public void DownloadLiveStreamChatLogic(string channel) {
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

                while (true) {
                    string inputLine;
                    while ((inputLine = reader.ReadLine()) != null) {
                        if (inputLine.Contains("PRIVMSG")) {
                            Console.WriteLine(SplitMessage(inputLine));
                        }

                        if (inputLine == "PING :tmi.twitch.tv") {
                            writer.WriteLine("PONG :tmi.twitch.tv");
                        }
                    }
                }

                // todo check emote compatibility; does it send offline notification in irc??
            }
        }

        public static string SplitMessage(string input) {
            var splitLine = input.Split(":");
            var name = splitLine[1].Split("!")[0];
            var message = String.Join(":", splitLine, 2, splitLine.Length - 2);
            return $"{name}: {message}";
        }
    }
}
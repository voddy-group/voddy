using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using voddy.Databases.Chat;
using voddy.Databases.Main;

namespace voddy.Controllers.Streams.Chat {
    public class GetStreamChatLogic {
        public string getChatLogic (long streamId) {
            List<Databases.Chat.Models.Chat> streamChat;
            using (var context = new ChatDataContext()) {
                streamChat = context.Chats.Where(item => item.streamId == streamId).ToList();
            }

            Directory.CreateDirectory(GlobalConfig.GetGlobalConfig("contentRootPath") + "tmp");
            string path = GlobalConfig.GetGlobalConfig("contentRootPath") + $"tmp/{streamId}.txt";
            File.WriteAllText(path, JsonConvert.SerializeObject(streamChat));
            
            return $"/tmp/{streamId}.txt";
        }
    }
}
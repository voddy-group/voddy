using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using voddy.Data;

namespace voddy.Controllers.Streams.Chat {
    public class GetStreamChatLogic {
        public string getChatLogic (long streamId) {
            List<Models.Chat> streamChat;
            string contentRootPath;
            using (var context = new DataContext()) {
                contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;
                streamChat = context.Chats.Where(item => item.streamId == streamId).ToList();
            }

            Directory.CreateDirectory(contentRootPath + "tmp");
            string path = contentRootPath + $"tmp/{streamId}.txt";
            File.WriteAllText(path, JsonConvert.SerializeObject(streamChat));
            
            return $"voddy/tmp/{streamId}.txt";
        }
    }
}
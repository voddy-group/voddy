using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using voddy.Controllers.Streams;
using voddy.Databases.Main;
using Stream = voddy.Databases.Main.Models.Stream;

namespace voddy.Controllers {
    public class CheckFiles {
        public CheckFiles() {
            using (var context = new MainDataContext()) {
                List<Stream> streams = context.Streams.ToList();
                string contentRootPath = context.Configs.FirstOrDefault(item => item.key == "contentRootPath").value;

                DeleteStreamsLogic deleteStreamsLogic = new DeleteStreamsLogic();
                
                for (var x = 0; x < streams.Count; x++) {
                    // make sure file is fully downloaded and the main vod does not exist
                    if (!streams[x].downloading && !File.Exists(Path.Combine(contentRootPath + streams[x].downloadLocation))) {
                        deleteStreamsLogic.DeleteSingleStreamLogic(streams[x].streamId);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
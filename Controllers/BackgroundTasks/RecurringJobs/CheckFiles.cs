using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using voddy.Controllers.Streams;
using voddy.Data;
using Stream = voddy.Models.Stream;

namespace voddy.Controllers {
    public class CheckFiles {
        public CheckFiles() {
            using (var context = new DataContext()) {
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
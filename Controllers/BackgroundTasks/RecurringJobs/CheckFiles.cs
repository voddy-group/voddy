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
                var streams = context.Streams.Where(item => item.downloading == false);

                DeleteStreamsLogic deleteStreamsLogic = new DeleteStreamsLogic();

                foreach (Stream stream in streams) {
                    if (!File.Exists(Path.Combine(GlobalConfig.GetGlobalConfig("contentRootPath") + stream.location + stream.fileName))) {
                        stream.missing = true;
                    } else if (stream.missing) {
                        stream.missing = false;
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
using System;

namespace voddy.Databases.Main.Models {
    public class Backup : MainDataContext.TableBase {
        public string type { get; set; }
        public DateTime datetime { get; set; }
        public string location { get; set; }
    }
}
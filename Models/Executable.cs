using voddy.Data;

namespace voddy.Models {
    public class Executable: DataContext.TableBase {
        public string name { get; set; }
        public string path { get; set; }
    }
}
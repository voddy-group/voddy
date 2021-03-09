using voddy.Data;

namespace voddy.Models {
    public class Config: DataContext.TableBase {
        public string key { get; set; }
        public string value { get; set; }
    }
}
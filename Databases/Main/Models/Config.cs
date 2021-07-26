namespace voddy.Databases.Main.Models {
    public class Config: MainDataContext.TableBase {
        public string key { get; set; }
        public string value { get; set; }
    }
}
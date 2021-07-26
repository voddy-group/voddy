namespace voddy.Databases.Main.Models {
    public class Authentication: MainDataContext.TableBase {
        public string service { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
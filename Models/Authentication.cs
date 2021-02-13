namespace voddy.Models {
    public class Authentication {
        public int id { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
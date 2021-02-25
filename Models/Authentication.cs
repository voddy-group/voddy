using voddy.Data;

namespace voddy.Models {
    public class Authentication: DataContext.TableBase {
        public string service { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
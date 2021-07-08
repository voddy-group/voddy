using System.Linq;
using voddy.Data;
using voddy.Models;

namespace voddy.Controllers.Setup.TwitchAuthentication {
    public class TwitchAuthLogic {


        public CredentialsReturn GetCredentialsLogic() {
            using (var context = new DataContext()) {
                Authentication authentication =
                    context.Authentications.FirstOrDefault(item => item.service == "twitch");

                if (authentication != null) {
                    return new CredentialsReturn {
                        clientId = authentication.clientId,
                        clientSecret = authentication.clientSecret
                    };
                } else {
                    return new CredentialsReturn();
                }
            }
        }
    }

    public class CredentialsReturn {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
    }
}
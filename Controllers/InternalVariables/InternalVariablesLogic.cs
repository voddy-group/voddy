using System.Collections.Generic;
using System.Linq;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.InternalVariables {
    public class InternalVariablesLogic {
        List<Config> configs = new();

        public List<Config> GetVariablesLogic() {
            configs.Add(new Config {
                key = "updateAvailable",
                value = GlobalConfig.GetGlobalConfig("updateAvailable")
            });
            configs.Add(new Config {
                key = "connectionError",
                value = GlobalConfig.GetGlobalConfig("connectionError")
            });

            return configs;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy.Controllers.InternalVariables {
    public class InternalVariablesLogic {
        List<Config> configs = new();

        public List<Config> GetVariablesLogic() {
            using (var context = new MainDataContext()) {
                GetConfig(context, "updateAvailable");
                GetConfig(context, "connectionError");
            }

            return configs;
        }
        
        private void GetConfig(MainDataContext context, string key) {
            var config = context.Configs.FirstOrDefault(item => item.key == key);
            if (config != null) {
                configs.Add(config);
            }
        }
    }
}
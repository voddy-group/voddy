using System;
using System.Collections.Generic;
using System.Linq;
using voddy.Databases.Main;
using voddy.Databases.Main.Models;

namespace voddy {
    public class GlobalConfig {
        public static List<Config> Config;

        public static void PopulateGlobalConfig() {
            using (var context = new MainDataContext()) {
                Config = context.Configs.ToList();
            }
        }

        public static string? GetGlobalConfig(string key) {
            var config = Config.FirstOrDefault(item => item.key == key);

            if (config != null) {
                return config.value;
            } else {
                return null;
            }
        }

        public static void SetGlobalConfig(string key, string value) {
            Config existingConfig;
            Config newConfig = new Config();

            using (var context = new MainDataContext()) {
                existingConfig = context.Configs.FirstOrDefault(item => item.key == key);
                if (existingConfig != null) {
                    existingConfig.value = value;
                } else {
                    newConfig = new Config {
                        key = key,
                        value = value
                    };

                    context.Add(newConfig);
                }


                context.SaveChanges();
            }

            // below must be done later to ensure no db errors
            if (existingConfig != null) {
                Config.FirstOrDefault(item => item.key == key).value = value;
            } else {
                // will already be set in the db context null check
                Config.Add(newConfig);
            }
        }

        public static void RemoveGlobalConfig(string key) {
            using (var context = new MainDataContext()) {
                Config record = context.Configs.FirstOrDefault(item => item.key == key);

                if (record != null) {
                    context.Remove(record);
                }

                context.SaveChanges();
            }
        }
    }
}
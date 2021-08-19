using Microsoft.EntityFrameworkCore;
using voddy.Databases.Main.Models;

namespace voddy.Databases.Logs {
    public class LogDataContext : DbContext {
        //public MainDataContext(DbContextOptions<MainDataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/storage/voddy/databases/logDb.db"); // todo change this
        }
        
        public DbSet<Log> Logs { get; set; } // logs
    }
}
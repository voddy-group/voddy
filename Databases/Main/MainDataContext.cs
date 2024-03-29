using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using voddy.Databases.Main.Models;

namespace voddy.Databases.Main {
    public class MainDataContext : DbContext {
        //public MainDataContext(DbContextOptions<MainDataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/storage/voddy/databases/mainDb.db"); // todo change this
        }

        public abstract class TableBase {
            [Key]
            public int id { get; set; }
        }
        
        public DbSet<Authentication> Authentications { get; set; } // API auth values. currently only used for twitch, but usage could be expanded
        public DbSet<Streamer> Streamers { get; set; } // streamers that the user added
        public DbSet<Config> Configs { get; set; } // general configs; key value storage
        public DbSet<Stream> Streams { get; set; } // downloaded stream/VOD info
        public DbSet<Backup> Backups { get; set; } // database backups for mainDb(this) and chatDb
        public DbSet<Notification> Notifications { get; set; } // application notifications
        public DbSet<JobTriggerExecution> JobTriggerExecutions { get; set; } // track last execution of quartz jobs for frontend page
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Streamer>()
                .HasIndex(u => u.streamerId)
                .IsUnique();
            modelBuilder.Entity<Stream>()
                .HasIndex(u => u.streamId)
                .IsUnique();
            modelBuilder.Entity<Stream>()
                .Property(item => item.missing)
                .HasDefaultValue(false);
            modelBuilder.Entity<JobTriggerExecution>()
                .HasIndex(u => new { u.Name, u.Group })
                .IsUnique();
            modelBuilder.Entity<Notification>()
                .HasIndex(u => u.id)
                .IsUnique();
        }
    }
}
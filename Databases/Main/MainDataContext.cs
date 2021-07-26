using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using voddy.Databases.Main.Models;

namespace voddy.Databases.Main {
    public class MainDataContext : DbContext {
        //public MainDataContext(DbContextOptions<MainDataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/storage/voddy/databases/mainDb.db"); // change this
        }

        public abstract class TableBase {
            [Key]
            public int id { get; set; }
        }
        
        public DbSet<Authentication> Authentications { get; set; }
        public DbSet<Streamer> Streamers { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Stream> Streams { get; set; }
        //public DbSet<Chat> Chats { get; set; }
        public DbSet<Log> Logs { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Streamer>()
                .HasIndex(u => u.streamerId)
                .IsUnique();
            modelBuilder.Entity<Stream>()
                .HasIndex(u => u.streamId)
                .IsUnique();
        }
    }
}
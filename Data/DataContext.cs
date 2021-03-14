using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using voddy.Models;

namespace voddy.Data {
    public class DataContext : DbContext {
        //public DataContext(DbContextOptions<DataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"User ID=postgres;Password=voddy12345;Server=localhost;Port=5432;Database=voddyDb;Integrated Security=true;Pooling=true;");
        }

        public abstract class TableBase {
            [Key]
            public int id { get; set; }
        }
        
        public DbSet<Authentication> Authentications { get; set; }
        public DbSet<Streamer> Streamers { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<Stream> Streams { get; set; }
        public DbSet<Chat> Chats { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Streamer>()
                .HasIndex(u => u.streamerId)
                .IsUnique();
            modelBuilder.Entity<Stream>()
                .HasIndex(u => u.streamId)
                .IsUnique();
            modelBuilder.Entity<Chat>()
                .HasIndex(u => u.streamId)
                .IsUnique();
        }
    }
}
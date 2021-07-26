using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace voddy.Databases.Chat {
    public class ChatDataContext : DbContext {
        //public DataContext(DbContextOptions<DataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/storage/voddy/databases/chatDb.db"); // change this
        }
        public DbSet<Models.Chat> Chats { get; set; }
        /*protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Streamer>()
                .HasIndex(u => u.streamerId)
                .IsUnique();
            modelBuilder.Entity<Stream>()
                .HasIndex(u => u.streamId)
                .IsUnique();
        }*/
    }
}
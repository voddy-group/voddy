using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace voddy.Databases.Chat {
    public class ChatDataContext : DbContext {
        //public DataContext(DbContextOptions<DataContext> options) : base (options) {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=/storage/voddy/databases/chatDb.db"); // todo change this
        }
        public DbSet<Models.Chat> Chats { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Models.Chat>()
                .HasIndex(u => u.messageId)
                .IsUnique();
        }
    }
}
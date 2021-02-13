using Microsoft.EntityFrameworkCore;
using voddy.Models;

namespace voddy.Data {
    public class DataContext : DbContext {
        public DataContext(DbContextOptions<DataContext> options) : base (options) {}
        
        public DbSet<Authentication> Authentications { get; set; }
    }
}
using lab10_PMS_API.Models;
using Microsoft.EntityFrameworkCore;

namespace lab10_PMS_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(
            DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
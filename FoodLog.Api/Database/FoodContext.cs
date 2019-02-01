using FoodLog.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FoodLog.Api.Database
{
    public class FoodContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public FoodContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbSet<Entry> Entries { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=C:\\Local-Databases\\Food.db");
            }
        }
    }
}

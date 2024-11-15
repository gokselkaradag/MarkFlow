using Microsoft.EntityFrameworkCore;

namespace RabbitMQWeb.Watermark.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options) 
        {

        }

        public DbSet<Product> Products { get; set; }
    }
}

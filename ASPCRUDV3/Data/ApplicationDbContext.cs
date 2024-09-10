using ASPCRUDV3.Models;
using Microsoft.EntityFrameworkCore;

namespace ASPCRUDV3.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<DataItem> DataItems { get; set; } // Setiap DbSet merepresentasikan tabel di database
    }
}

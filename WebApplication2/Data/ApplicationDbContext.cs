using Microsoft.EntityFrameworkCore;
using RabbitAdoption.ProducerAPI.Models;

namespace RabbitAdoption.ProducerAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<AdoptionRequest> AdoptionRequests { get; set; }
    }
}

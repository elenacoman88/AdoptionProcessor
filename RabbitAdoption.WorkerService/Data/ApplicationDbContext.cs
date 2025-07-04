using Microsoft.EntityFrameworkCore;
using RabbitAdoption.WorkerService.Models;

namespace RabbitAdoption.WorkerService.Data
{
    public class ApplicationDbContext : DbContext
    {
        //public ApplicationDbContext()
        //{
            
        //}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer(
        //        sqlOptions => sqlOptions.CommandTimeout(60) // seconds
        //    );
        //}
        public DbSet<Rabbit> Rabbits { get; set; }
    }
}

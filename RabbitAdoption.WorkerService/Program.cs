using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RabbitAdoption.WorkerService;
using RabbitAdoption.WorkerService.Data;
using System;

var builder = Host.CreateApplicationBuilder(args);



builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.CommandTimeout(60));
});
//var optionBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

//var connectionString = "Server=LAPTOP-91O90D8E\\SQLEXPRESS;Database=Adoption;Trusted_Connection=True;TrustServerCertificate=True";


builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<AdoptionRequestProcessor>();

builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();

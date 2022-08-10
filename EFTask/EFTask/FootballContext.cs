using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFTask
{
    public class FootballContext : DbContext
    {
        public DbSet<FootballPlayer> Players { get; set; } = null!;
        public DbSet<FootballTeam> Teams { get; set; } = null!;
        public DbSet<FootbalContract> Contracts { get; set; } = null!;

        public FootballContext()
        {
            Console.WriteLine("Connecting to database...");
            Database.EnsureCreated();
            Console.WriteLine("Connection established");
            Console.WriteLine();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = GetConnectionString();
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        private static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder();
            var config = builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var dbConnectionSettings = config.GetSection("databaseConnectionSettings");
            string host = dbConnectionSettings["host"],
                port = dbConnectionSettings["port"],
                db = dbConnectionSettings["database"],
                user = dbConnectionSettings["username"],
                password = dbConnectionSettings["password"];
            return $"Host={host};Port={port};Database={db};Username={user};Password={password};";
        }
    }
}

using EFTask.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
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
        public IDatabase RedisCache { get; protected set; } = null!;

        public FootballContext()
        {
            Console.WriteLine("Ensuring that the database exists...");
            Database.EnsureCreated();
            Console.WriteLine("Success");
            Console.Clear();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Console.WriteLine("Connecting to database...");
            var builder = new ConfigurationBuilder();
            var config = builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var psqlConnection = config.GetConnectionString("Postgres");
            optionsBuilder.UseNpgsql(psqlConnection);
            Console.WriteLine("Connection established");

            Console.WriteLine("Connecting to redis...");
            var redisConnection = config.GetConnectionString("Redis");
            RedisCache = ConnectionMultiplexer.Connect(redisConnection).GetDatabase();
            Console.WriteLine("Connection established");
        }
    }
}

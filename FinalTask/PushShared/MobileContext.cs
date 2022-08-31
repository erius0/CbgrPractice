using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PushShared.Mobile.Data;

namespace PushShared;

public class MobileContext : DbContext
{
    public DbSet<MobileAppUser> MobileAppUsers { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    private readonly ILogger<MobileContext> _logger;
    private readonly IConfiguration _configuration;

    public MobileContext(ILogger<MobileContext> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _logger.LogInformation("Ensuring that the database exists...");
        Database.EnsureCreated();
        _logger.LogInformation("Success");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _logger.LogInformation("Connecting to database...");
        optionsBuilder.UseNpgsql(_configuration.GetConnectionString("Postgres"));
        _logger.LogInformation("Connection established");
    }
}

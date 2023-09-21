using DigitArenaBot.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot;

public class DefaultDatabaseContext : DbContext
{
    
    private IConfigurationRoot _config;
    
    public DefaultDatabaseContext(DbContextOptions<DefaultDatabaseContext> options, IConfigurationRoot config) : base(options)
    {
        _config = config;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = _config["ConnectionStrings:Db"];
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
    
    // public DbSet<MessageReactionCount> MessageReactionCounts { get; set; }
    public DbSet<ArchivedMessages> ArchivedMessages { get; set; }
}
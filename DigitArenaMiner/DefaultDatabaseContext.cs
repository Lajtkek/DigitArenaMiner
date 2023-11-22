using DigitArenaBot.Classes.Game;
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
        var connectionString =
            "Server=monorail.proxy.rlwy.net;Port=33106;Database=railway;Uid=postgres;Pwd=bfA3a24DEFB-2A1A6Gdgf5*EDd2ecFge"; //Environment.GetEnvironmentVariable("CONNECTION_STRING");
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PollQuestion>().HasMany(x => x.Answers).WithOne(x => x.Parent);
    }
    
    public DbSet<MessageReactionCount> MessageReactionCounts { get; set; }
    public DbSet<ArchivedMessages> ArchivedMessages { get; set; }
    public DbSet<UserActionCount> UserActionCounts { get; set; }
    public DbSet<CumRecord> CumRecords { get; set; }
    
    
    public DbSet<PollQuestion> Questions { get; set; }
    public DbSet<PollAnswer> Answers { get; set; }
    public DbSet<UserPollAnswer> UserPollAnswers { get; set; }
}
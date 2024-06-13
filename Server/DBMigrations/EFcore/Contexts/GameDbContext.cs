using Microsoft.EntityFrameworkCore;
using Server.EFcore.Models;

namespace Server.EFcore.Contexts
{
    public class GameDbContext : DbContext
    {
        readonly string connectionString = string.Empty;
        public DbSet<GameAccount> Accounts { get; set; }
        public DbSet<CharacterData> Characters { get; set; }
        public DbSet<GameWorld> GameWorlds { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<EquipItem> EquipItems { get; set; }

        public GameDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GameAccount>().HasKey(x => x.Id);
            modelBuilder.Entity<GameAccount>()
                .HasIndex(x => x.LoginId)
                .IsUnique();

            modelBuilder.Entity<GameAccount>()
                .HasOne(e => e.GameWorld)
                .WithOne(e => e.Owner)
                .HasForeignKey<GameWorld>(e => e.Id)
                .IsRequired();

            modelBuilder.Entity<Item>()
                .HasOne(x => x.Owner)
                .WithMany()
                .IsRequired();

            modelBuilder.Entity<CompletedAchievement>()
                .HasOne(x => x.GameWorld)
                .WithMany(x => x.Achievements)
                .HasForeignKey(x => x.GameWorldId)
                .IsRequired();


            modelBuilder.Entity<CompletedAchievement>()
                .HasIndex(e => new { e.AchievementCode, e.GameWorldId })
                .IsUnique();

            modelBuilder.Entity<GameAccount>()
                .Property(x => x.AccountLevel)
                .HasDefaultValue(1);

            base.OnModelCreating(modelBuilder);
        }
    }
}

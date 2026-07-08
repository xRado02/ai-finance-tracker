using AiFinanceTracker.Domain;
using Microsoft.EntityFrameworkCore;

namespace AiFinanceTracker.Persistence;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public DbSet<LocalProfile> LocalProfiles => Set<LocalProfile>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalProfile>(profile =>
        {
            profile.HasKey(p => p.Id);
            profile.Property(p => p.DisplayName)
                .HasMaxLength(120)
                .IsRequired();
        });

        modelBuilder.Entity<Category>(category =>
        {
            category.HasKey(c => c.Id);
            category.Property(c => c.Name)
                .HasMaxLength(80)
                .IsRequired();
            category.HasIndex(c => c.Name)
                .IsUnique();
            category.Property(c => c.AppliesTo)
                .HasConversion<string>()
                .HasMaxLength(20);
        });

        modelBuilder.Entity<Transaction>(transaction =>
        {
            transaction.HasKey(t => t.Id);
            transaction.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();
            transaction.Property(t => t.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();
            transaction.Property(t => t.TransactionDate)
                .IsRequired();
            transaction.Property(t => t.Description)
                .HasMaxLength(500);
            transaction.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            transaction.HasOne(t => t.LocalProfile)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.LocalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

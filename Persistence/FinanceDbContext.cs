using AiFinanceTracker.Domain;
using Microsoft.EntityFrameworkCore;

namespace AiFinanceTracker.Persistence;

public sealed class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : DbContext(options)
{
    public static readonly Guid DefaultLocalProfileId = new("11111111-1111-1111-1111-111111111111");

    public static readonly Guid OtherCategoryId = new("20000000-0000-0000-0000-000000000001");
    public static readonly Guid FoodCategoryId = new("20000000-0000-0000-0000-000000000002");
    public static readonly Guid TransportCategoryId = new("20000000-0000-0000-0000-000000000003");
    public static readonly Guid HousingCategoryId = new("20000000-0000-0000-0000-000000000004");
    public static readonly Guid BillsCategoryId = new("20000000-0000-0000-0000-000000000005");
    public static readonly Guid EntertainmentCategoryId = new("20000000-0000-0000-0000-000000000006");
    public static readonly Guid HealthCategoryId = new("20000000-0000-0000-0000-000000000007");
    public static readonly Guid SalaryCategoryId = new("20000000-0000-0000-0000-000000000008");
    public static readonly Guid OtherIncomeCategoryId = new("20000000-0000-0000-0000-000000000009");

    public DbSet<LocalProfile> LocalProfiles => Set<LocalProfile>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<Goal> Goals => Set<Goal>();

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

        modelBuilder.Entity<Goal>(goal =>
        {
            goal.HasKey(item => item.Id);
            goal.Property(item => item.Name)
                .HasMaxLength(120)
                .IsRequired();
            goal.Property(item => item.TargetAmount)
                .HasPrecision(18, 2)
                .IsRequired();
            goal.HasOne(item => item.LocalProfile)
                .WithMany(profile => profile.Goals)
                .HasForeignKey(item => item.LocalProfileId)
                .OnDelete(DeleteBehavior.Cascade);
            goal.HasIndex(item => item.LocalProfileId);
        });

        SeedLocalProfile(modelBuilder);
        SeedCategories(modelBuilder);
    }

    private static void SeedLocalProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LocalProfile>().HasData(new LocalProfile
        {
            Id = DefaultLocalProfileId,
            DisplayName = "Default Local Profile"
        });
    }

    private static void SeedCategories(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = OtherCategoryId, Name = "Other", AppliesTo = null },
            new Category { Id = FoodCategoryId, Name = "Food", AppliesTo = TransactionType.Expense },
            new Category { Id = TransportCategoryId, Name = "Transport", AppliesTo = TransactionType.Expense },
            new Category { Id = HousingCategoryId, Name = "Housing", AppliesTo = TransactionType.Expense },
            new Category { Id = BillsCategoryId, Name = "Bills", AppliesTo = TransactionType.Expense },
            new Category { Id = EntertainmentCategoryId, Name = "Entertainment", AppliesTo = TransactionType.Expense },
            new Category { Id = HealthCategoryId, Name = "Health", AppliesTo = TransactionType.Expense },
            new Category { Id = SalaryCategoryId, Name = "Salary", AppliesTo = TransactionType.Income },
            new Category { Id = OtherIncomeCategoryId, Name = "Other Income", AppliesTo = TransactionType.Income });
    }
}

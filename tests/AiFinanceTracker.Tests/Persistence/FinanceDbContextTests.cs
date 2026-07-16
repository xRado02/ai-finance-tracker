using AiFinanceTracker.Domain;
using AiFinanceTracker.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AiFinanceTracker.Tests.Persistence;

public sealed class FinanceDbContextTests
{
    [Fact]
    public void Model_contains_required_persistence_entities_and_relationships()
    {
        using var connection = CreateOpenConnection();
        using var context = CreateContext(connection);

        var model = context.Model;
        var profile = model.FindEntityType(typeof(LocalProfile));
        var transaction = model.FindEntityType(typeof(Transaction));
        var goal = model.FindEntityType(typeof(Goal));
        var recurring = model.FindEntityType(typeof(RecurringTransaction));

        Assert.NotNull(profile);
        Assert.NotNull(model.FindEntityType(typeof(Category)));
        Assert.NotNull(transaction);
        Assert.NotNull(goal);
        Assert.NotNull(recurring);
        Assert.NotNull(transaction.FindNavigation(nameof(Transaction.LocalProfile)));
        Assert.NotNull(transaction.FindNavigation(nameof(Transaction.Category)));
        Assert.NotNull(goal.FindNavigation(nameof(Goal.LocalProfile)));
        Assert.NotNull(transaction.FindNavigation(nameof(Transaction.RecurringTransaction)));
        Assert.NotNull(recurring.FindNavigation(nameof(RecurringTransaction.LocalProfile)));

        var amount = transaction.FindProperty(nameof(Transaction.Amount));
        Assert.NotNull(amount);
        Assert.Equal(18, amount.GetPrecision());
        Assert.Equal(2, amount.GetScale());

        var targetAmount = goal.FindProperty(nameof(Goal.TargetAmount));
        Assert.NotNull(targetAmount);
        Assert.Equal(18, targetAmount.GetPrecision());
        Assert.Equal(2, targetAmount.GetScale());

        var recurringAmount = recurring.FindProperty(nameof(RecurringTransaction.Amount));
        Assert.NotNull(recurringAmount);
        Assert.Equal(18, recurringAmount.GetPrecision());
        Assert.Equal(2, recurringAmount.GetScale());

        var initialBalance = profile.FindProperty(nameof(LocalProfile.InitialBalance));
        Assert.NotNull(initialBalance);
        Assert.Equal(18, initialBalance.GetPrecision());
        Assert.Equal(2, initialBalance.GetScale());
    }

    [Fact]
    public async Task Seed_contains_default_local_profile()
    {
        using var connection = CreateOpenConnection();
        using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var profile = await context.LocalProfiles.SingleAsync();

        Assert.Equal(FinanceDbContext.DefaultLocalProfileId, profile.Id);
        Assert.Equal("Default Local Profile", profile.DisplayName);
        Assert.Equal(0m, profile.InitialBalance);
    }

    [Fact]
    public async Task Seed_contains_required_startup_categories()
    {
        using var connection = CreateOpenConnection();
        using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var categoryNames = await context.Categories
            .OrderBy(category => category.Name)
            .Select(category => category.Name)
            .ToListAsync();

        Assert.Equal(
            [
                "Bills",
                "Entertainment",
                "Food",
                "Health",
                "Housing",
                "Other",
                "Other Income",
                "Salary",
                "Transport"
            ],
            categoryNames);

        Assert.Contains(await context.Categories.ToListAsync(), category =>
            category.Id == FinanceDbContext.OtherCategoryId &&
            category.Name == "Other" &&
            category.AppliesTo is null);
    }

    [Fact]
    public async Task Can_save_and_read_transaction_for_seeded_profile_and_category()
    {
        using var connection = CreateOpenConnection();
        await using var arrangeContext = CreateContext(connection);
        await arrangeContext.Database.EnsureCreatedAsync();

        var transaction = new Transaction
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            Amount = 125.50m,
            Type = TransactionType.Expense,
            TransactionDate = new DateOnly(2026, 7, 8),
            Description = "Groceries",
            CategoryId = FinanceDbContext.FoodCategoryId,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };

        arrangeContext.Transactions.Add(transaction);
        await arrangeContext.SaveChangesAsync();

        await using var assertContext = CreateContext(connection);
        var saved = await assertContext.Transactions
            .Include(item => item.Category)
            .Include(item => item.LocalProfile)
            .SingleAsync(item => item.Id == transaction.Id);

        Assert.Equal(125.50m, saved.Amount);
        Assert.Equal(TransactionType.Expense, saved.Type);
        Assert.Equal(new DateOnly(2026, 7, 8), saved.TransactionDate);
        Assert.Equal("Groceries", saved.Description);
        Assert.Equal(FinanceDbContext.FoodCategoryId, saved.CategoryId);
        Assert.Equal(FinanceDbContext.DefaultLocalProfileId, saved.LocalProfileId);
        Assert.Equal("Food", saved.Category?.Name);
        Assert.Equal("Default Local Profile", saved.LocalProfile?.DisplayName);
    }

    private static SqliteConnection CreateOpenConnection()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }

    private static FinanceDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseSqlite(connection)
            .Options;

        return new FinanceDbContext(options);
    }
}

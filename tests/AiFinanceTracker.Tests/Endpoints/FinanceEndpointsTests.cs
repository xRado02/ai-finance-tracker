using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiFinanceTracker.Contracts;
using AiFinanceTracker.Domain;
using AiFinanceTracker.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AiFinanceTracker.Tests.Endpoints;

public sealed class FinanceEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task Get_categories_returns_seeded_categories_including_other()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();

        var response = await client.GetAsync("/api/categories");

        response.EnsureSuccessStatusCode();
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>(JsonOptions);

        Assert.NotNull(categories);
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
            categories.Select(category => category.Name).ToList());
        Assert.Contains(categories, category =>
            category.Id == FinanceDbContext.OtherCategoryId &&
            category.Name == "Other" &&
            category.AppliesTo is null);
    }

    [Fact]
    public async Task Post_transactions_creates_expense_for_default_profile()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateTransactionRequest(
            125.50m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 8),
            "Groceries",
            FinanceDbContext.FoodCategoryId);

        var response = await client.PostAsJsonAsync("/api/transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        Assert.Equal("Expense", json.RootElement.GetProperty("type").GetString());

        var created = JsonSerializer.Deserialize<TransactionResponse>(content, JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(125.50m, created.Amount);
        Assert.Equal(TransactionType.Expense, created.Type);
        Assert.Equal(new DateOnly(2026, 7, 8), created.TransactionDate);
        Assert.Equal("Groceries", created.Description);
        Assert.Equal(FinanceDbContext.FoodCategoryId, created.CategoryId);
        Assert.Equal("Food", created.CategoryName);

        await using var dbContext = app.CreateDbContext();
        var saved = await dbContext.Transactions.SingleAsync(transaction => transaction.Id == created.Id);

        Assert.Equal(FinanceDbContext.DefaultLocalProfileId, saved.LocalProfileId);
        Assert.Equal(FinanceDbContext.FoodCategoryId, saved.CategoryId);
    }

    [Fact]
    public async Task Get_transactions_returns_newest_first_and_honors_limit()
    {
        using var app = new FinanceApiFactory();
        await app.SeedTransactionsAsync(
            CreateTransaction("30000000-0000-0000-0000-000000000001", 10m, new DateOnly(2026, 7, 1)),
            CreateTransaction("30000000-0000-0000-0000-000000000002", 20m, new DateOnly(2026, 7, 3)),
            CreateTransaction("30000000-0000-0000-0000-000000000003", 30m, new DateOnly(2026, 7, 2)));
        using var client = app.CreateClient();

        var transactions = await client.GetFromJsonAsync<List<TransactionResponse>>("/api/transactions?limit=2", JsonOptions);

        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count);
        Assert.Equal(
            [new DateOnly(2026, 7, 3), new DateOnly(2026, 7, 2)],
            transactions.Select(transaction => transaction.TransactionDate).ToList());
        Assert.All(transactions, transaction => Assert.Equal("Food", transaction.CategoryName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task Get_transactions_rejects_invalid_limit(int limit)
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();

        var response = await client.GetAsync($"/api/transactions?limit={limit}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemAsync(response, "limit");
    }

    [Fact]
    public async Task Delete_transactions_removes_transaction_from_default_profile()
    {
        using var app = new FinanceApiFactory();
        var transaction = CreateTransaction(
            "30000000-0000-0000-0000-000000000010",
            42m,
            new DateOnly(2026, 7, 8));
        await app.SeedTransactionsAsync(transaction);
        using var client = app.CreateClient();

        var response = await client.DeleteAsync($"/api/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await using var dbContext = app.CreateDbContext();
        Assert.False(await dbContext.Transactions.AnyAsync(item => item.Id == transaction.Id));
    }

    [Fact]
    public async Task Delete_transactions_returns_not_found_for_missing_transaction()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var id = Guid.Parse("30000000-0000-0000-0000-000000000011");

        var response = await client.DeleteAsync($"/api/transactions/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Transaction not found");
    }

    [Fact]
    public async Task Delete_transactions_does_not_remove_transaction_from_another_profile()
    {
        using var app = new FinanceApiFactory();
        var otherProfileId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        var transaction = CreateTransaction(
            "30000000-0000-0000-0000-000000000012",
            84m,
            new DateOnly(2026, 7, 9));
        transaction.LocalProfileId = otherProfileId;

        await using (var dbContext = app.CreateDbContext())
        {
            dbContext.LocalProfiles.Add(new LocalProfile
            {
                Id = otherProfileId,
                DisplayName = "Other Local Profile"
            });
            dbContext.Transactions.Add(transaction);
            await dbContext.SaveChangesAsync();
        }

        using var client = app.CreateClient();
        var response = await client.DeleteAsync($"/api/transactions/{transaction.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await using var verificationContext = app.CreateDbContext();
        Assert.True(await verificationContext.Transactions.AnyAsync(item => item.Id == transaction.Id));
    }

    [Fact]
    public async Task Post_goals_creates_goal_for_default_profile_with_initial_progress()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateGoalRequest("Emergency fund", 10000m);

        var response = await client.PostAsJsonAsync("/api/goals", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var goal = await response.Content.ReadFromJsonAsync<GoalResponse>(JsonOptions);
        Assert.NotNull(goal);
        Assert.Equal("Emergency fund", goal.Name);
        Assert.Equal(10000m, goal.TargetAmount);
        Assert.Equal(0m, goal.CurrentAmount);
        Assert.Equal(0m, goal.ProgressPercentage);

        await using var dbContext = app.CreateDbContext();
        var saved = await dbContext.Goals.SingleAsync(item => item.Id == goal.Id);
        Assert.Equal(FinanceDbContext.DefaultLocalProfileId, saved.LocalProfileId);
    }

    [Fact]
    public async Task Get_goals_calculates_current_amount_and_progress_from_default_profile_transactions()
    {
        using var app = new FinanceApiFactory();
        var income = CreateTransaction(
            "30000000-0000-0000-0000-000000000020",
            1000m,
            new DateOnly(2026, 7, 10));
        income.Type = TransactionType.Income;
        income.CategoryId = FinanceDbContext.SalaryCategoryId;
        var expense = CreateTransaction(
            "30000000-0000-0000-0000-000000000021",
            200m,
            new DateOnly(2026, 7, 11));
        await app.SeedTransactionsAsync(income, expense);
        using var client = app.CreateClient();
        await client.PostAsJsonAsync(
            "/api/goals",
            new CreateGoalRequest("Emergency fund", 1600m),
            JsonOptions);

        var response = await client.GetAsync("/api/goals");

        response.EnsureSuccessStatusCode();
        var goals = await response.Content.ReadFromJsonAsync<List<GoalResponse>>(JsonOptions);
        Assert.NotNull(goals);
        var goal = Assert.Single(goals);
        Assert.Equal(800m, goal.CurrentAmount);
        Assert.Equal(50m, goal.ProgressPercentage);
    }

    [Fact]
    public async Task Post_goals_rejects_invalid_name_and_target_amount()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/goals",
            new CreateGoalRequest("", 0m),
            JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemAsync(response, "Name", "TargetAmount");
    }

    [Fact]
    public async Task Get_goals_returns_only_goals_from_default_profile()
    {
        using var app = new FinanceApiFactory();
        var otherProfileId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        await using (var dbContext = app.CreateDbContext())
        {
            dbContext.LocalProfiles.Add(new LocalProfile
            {
                Id = otherProfileId,
                DisplayName = "Other Local Profile"
            });
            dbContext.Goals.Add(new Goal
            {
                Id = Guid.Parse("50000000-0000-0000-0000-000000000001"),
                Name = "Other profile goal",
                TargetAmount = 500m,
                LocalProfileId = otherProfileId
            });
            await dbContext.SaveChangesAsync();
        }

        using var client = app.CreateClient();
        var response = await client.GetAsync("/api/goals");

        response.EnsureSuccessStatusCode();
        var goals = await response.Content.ReadFromJsonAsync<List<GoalResponse>>(JsonOptions);
        Assert.NotNull(goals);
        Assert.Empty(goals);
    }

    [Fact]
    public async Task Get_dashboard_summary_returns_totals_categories_and_goal_progress()
    {
        using var app = new FinanceApiFactory();
        var income = CreateTransaction(
            "30000000-0000-0000-0000-000000000030",
            1000m,
            new DateOnly(2026, 7, 12));
        income.Type = TransactionType.Income;
        income.CategoryId = FinanceDbContext.SalaryCategoryId;
        var food = CreateTransaction(
            "30000000-0000-0000-0000-000000000031",
            200m,
            new DateOnly(2026, 7, 13));
        await app.SeedTransactionsAsync(income, food);
        using var client = app.CreateClient();
        await client.PostAsJsonAsync(
            "/api/goals",
            new CreateGoalRequest("Emergency fund", 1600m),
            JsonOptions);

        var response = await client.GetAsync("/api/dashboard/summary");

        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>(JsonOptions);
        Assert.NotNull(summary);
        Assert.Equal(1000m, summary.TotalIncome);
        Assert.Equal(200m, summary.TotalExpenses);
        Assert.Equal(800m, summary.Balance);
        var category = Assert.Single(summary.ExpenseCategories);
        Assert.Equal("Food", category.CategoryName);
        Assert.Equal(200m, category.Amount);
        var goal = Assert.Single(summary.Goals);
        Assert.Equal(800m, goal.CurrentAmount);
        Assert.Equal(50m, goal.ProgressPercentage);
    }

    [Fact]
    public async Task Get_dashboard_summary_ignores_transactions_from_another_profile()
    {
        using var app = new FinanceApiFactory();
        var otherProfileId = Guid.Parse("40000000-0000-0000-0000-000000000003");
        await using (var dbContext = app.CreateDbContext())
        {
            dbContext.LocalProfiles.Add(new LocalProfile
            {
                Id = otherProfileId,
                DisplayName = "Other Local Profile"
            });
            dbContext.Transactions.Add(new Transaction
            {
                Id = Guid.Parse("30000000-0000-0000-0000-000000000032"),
                Amount = 999m,
                Type = TransactionType.Income,
                TransactionDate = new DateOnly(2026, 7, 13),
                CategoryId = FinanceDbContext.SalaryCategoryId,
                LocalProfileId = otherProfileId
            });
            await dbContext.SaveChangesAsync();
        }

        using var client = app.CreateClient();
        var response = await client.GetAsync("/api/dashboard/summary");

        response.EnsureSuccessStatusCode();
        var summary = await response.Content.ReadFromJsonAsync<DashboardSummaryResponse>(JsonOptions);
        Assert.NotNull(summary);
        Assert.Equal(0m, summary.TotalIncome);
        Assert.Equal(0m, summary.TotalExpenses);
        Assert.Equal(0m, summary.Balance);
        Assert.Empty(summary.ExpenseCategories);
    }

    [Fact]
    public async Task Post_recurring_transactions_creates_definition_for_default_profile()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateRecurringTransactionRequest(
            120m,
            TransactionType.Expense,
            FinanceDbContext.BillsCategoryId,
            "Internet",
            true);

        var response = await client.PostAsJsonAsync("/api/recurring-transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var recurring = await response.Content.ReadFromJsonAsync<RecurringTransactionResponse>(JsonOptions);
        Assert.NotNull(recurring);
        Assert.Equal(120m, recurring.Amount);
        Assert.Equal(TransactionType.Expense, recurring.Type);
        Assert.Equal("Bills", recurring.CategoryName);
        Assert.True(recurring.IsActive);

        await using var dbContext = app.CreateDbContext();
        var saved = await dbContext.RecurringTransactions.SingleAsync(item => item.Id == recurring.Id);
        Assert.Equal(FinanceDbContext.DefaultLocalProfileId, saved.LocalProfileId);
    }

    [Fact]
    public async Task Patch_recurring_transaction_status_updates_default_profile_definition()
    {
        using var app = new FinanceApiFactory();
        var recurring = CreateRecurringTransaction(
            "60000000-0000-0000-0000-000000000001",
            90m,
            FinanceDbContext.BillsCategoryId,
            true);
        await app.SeedRecurringTransactionsAsync(recurring);
        using var client = app.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/recurring-transactions/{recurring.Id}/status",
            new UpdateRecurringTransactionStatusRequest(false),
            JsonOptions);

        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<RecurringTransactionResponse>(JsonOptions);
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
    }

    [Fact]
    public async Task Generate_current_month_creates_active_definitions_once_and_skips_inactive()
    {
        using var app = new FinanceApiFactory();
        var active = CreateRecurringTransaction(
            "60000000-0000-0000-0000-000000000002",
            250m,
            FinanceDbContext.SalaryCategoryId,
            true);
        active.Type = TransactionType.Income;
        var inactive = CreateRecurringTransaction(
            "60000000-0000-0000-0000-000000000003",
            80m,
            FinanceDbContext.FoodCategoryId,
            false);
        await app.SeedRecurringTransactionsAsync(active, inactive);
        using var client = app.CreateClient();

        var firstResponse = await client.PostAsync("/api/recurring-transactions/generate-current-month", null);
        firstResponse.EnsureSuccessStatusCode();
        var first = await firstResponse.Content.ReadFromJsonAsync<GenerateRecurringTransactionsResponse>(JsonOptions);
        Assert.NotNull(first);
        Assert.Equal(1, first.GeneratedCount);
        Assert.Equal(0, first.SkippedCount);
        var generatedTransaction = Assert.Single(first.Transactions);
        Assert.Equal(active.Id, (await ReadGeneratedTransactionAsync(app, generatedTransaction.Id)).RecurringTransactionId);

        var secondResponse = await client.PostAsync("/api/recurring-transactions/generate-current-month", null);
        secondResponse.EnsureSuccessStatusCode();
        var second = await secondResponse.Content.ReadFromJsonAsync<GenerateRecurringTransactionsResponse>(JsonOptions);
        Assert.NotNull(second);
        Assert.Equal(0, second.GeneratedCount);
        Assert.Equal(1, second.SkippedCount);

        await using var dbContext = app.CreateDbContext();
        Assert.Equal(1, await dbContext.Transactions.CountAsync());
        var monthStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        Assert.Equal(monthStart, (await dbContext.Transactions.SingleAsync()).TransactionDate);
    }

    [Fact]
    public async Task Generate_current_month_ignores_recurring_definitions_from_another_profile()
    {
        using var app = new FinanceApiFactory();
        var otherProfileId = Guid.Parse("40000000-0000-0000-0000-000000000004");
        await using (var dbContext = app.CreateDbContext())
        {
            dbContext.LocalProfiles.Add(new LocalProfile
            {
                Id = otherProfileId,
                DisplayName = "Other Local Profile"
            });
            dbContext.RecurringTransactions.Add(new RecurringTransaction
            {
                Id = Guid.Parse("60000000-0000-0000-0000-000000000004"),
                Amount = 999m,
                Type = TransactionType.Income,
                CategoryId = FinanceDbContext.SalaryCategoryId,
                IsActive = true,
                LocalProfileId = otherProfileId
            });
            await dbContext.SaveChangesAsync();
        }

        using var client = app.CreateClient();
        var response = await client.PostAsync("/api/recurring-transactions/generate-current-month", null);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<GenerateRecurringTransactionsResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.Equal(0, result.GeneratedCount);
        await using var verificationContext = app.CreateDbContext();
        Assert.Empty(await verificationContext.Transactions.ToListAsync());
    }

    [Fact]
    public async Task Post_recurring_transactions_rejects_invalid_amount_and_description()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateRecurringTransactionRequest(
            0m,
            TransactionType.Expense,
            FinanceDbContext.FoodCategoryId,
            new string('x', 501),
            true);

        var response = await client.PostAsJsonAsync("/api/recurring-transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemAsync(response, "Amount", "Description");
    }

    [Fact]
    public async Task Post_transactions_rejects_invalid_request()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateTransactionRequest(
            0m,
            TransactionType.Expense,
            default,
            new string('x', 501),
            FinanceDbContext.FoodCategoryId);

        var response = await client.PostAsJsonAsync("/api/transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemAsync(response, "Amount", "TransactionDate", "Description");
    }

    [Fact]
    public async Task Post_transactions_rejects_missing_category()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateTransactionRequest(
            25m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 8),
            null,
            Guid.Parse("99999999-9999-9999-9999-999999999999"));

        var response = await client.PostAsJsonAsync("/api/transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Category not found");
    }

    [Fact]
    public async Task Post_transactions_rejects_category_type_mismatch()
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateTransactionRequest(
            25m,
            TransactionType.Expense,
            new DateOnly(2026, 7, 8),
            null,
            FinanceDbContext.SalaryCategoryId);

        var response = await client.PostAsJsonAsync("/api/transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertValidationProblemAsync(response, "CategoryId");
    }

    [Theory]
    [InlineData(TransactionType.Income)]
    [InlineData(TransactionType.Expense)]
    public async Task Post_transactions_accepts_other_for_income_and_expense(TransactionType type)
    {
        using var app = new FinanceApiFactory();
        using var client = app.CreateClient();
        var request = new CreateTransactionRequest(
            25m,
            type,
            new DateOnly(2026, 7, 8),
            null,
            FinanceDbContext.OtherCategoryId);

        var response = await client.PostAsJsonAsync("/api/transactions", request, JsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<TransactionResponse>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal("Other", created.CategoryName);
        Assert.Equal(type, created.Type);
    }

    private static Transaction CreateTransaction(string id, decimal amount, DateOnly date)
    {
        return new Transaction
        {
            Id = Guid.Parse(id),
            Amount = amount,
            Type = TransactionType.Expense,
            TransactionDate = date,
            CategoryId = FinanceDbContext.FoodCategoryId,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };
    }

    private static RecurringTransaction CreateRecurringTransaction(
        string id,
        decimal amount,
        Guid categoryId,
        bool isActive)
    {
        return new RecurringTransaction
        {
            Id = Guid.Parse(id),
            Amount = amount,
            Type = TransactionType.Expense,
            CategoryId = categoryId,
            Description = "Recurring",
            IsActive = isActive,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };
    }

    private static async Task<Transaction> ReadGeneratedTransactionAsync(FinanceApiFactory app, Guid id)
    {
        await using var dbContext = app.CreateDbContext();
        return await dbContext.Transactions.SingleAsync(item => item.Id == id);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private static async Task AssertValidationProblemAsync(HttpResponseMessage response, params string[] errorKeys)
    {
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal((int)HttpStatusCode.BadRequest, root.GetProperty("status").GetInt32());
        var errors = root.GetProperty("errors");
        foreach (var errorKey in errorKeys)
        {
            Assert.True(
                errors.TryGetProperty(errorKey, out var messages),
                $"Expected validation problem to include '{errorKey}'.");
            Assert.True(messages.GetArrayLength() > 0, $"Expected '{errorKey}' to include at least one message.");
        }
    }

    private static async Task AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedTitle)
    {
        using var json = await ReadJsonAsync(response);
        var root = json.RootElement;

        Assert.Equal((int)expectedStatus, root.GetProperty("status").GetInt32());
        Assert.Equal(expectedTitle, root.GetProperty("title").GetString());
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(content);
    }

    private sealed class FinanceApiFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection connection = new("DataSource=:memory:");

        public FinanceApiFactory()
        {
            connection.Open();
        }

        public FinanceDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<FinanceDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new FinanceDbContext(options);
            dbContext.Database.EnsureCreated();

            return dbContext;
        }

        public async Task SeedTransactionsAsync(params Transaction[] transactions)
        {
            await using var dbContext = CreateDbContext();
            dbContext.Transactions.AddRange(transactions);
            await dbContext.SaveChangesAsync();
        }

        public async Task SeedRecurringTransactionsAsync(params RecurringTransaction[] recurringTransactions)
        {
            await using var dbContext = CreateDbContext();
            dbContext.RecurringTransactions.AddRange(recurringTransactions);
            await dbContext.SaveChangesAsync();
        }

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ILoggerProvider>();
                services.RemoveAll<FinanceDbContext>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<DbContextOptions<FinanceDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<FinanceDbContext>>();
                services.AddDbContext<FinanceDbContext>(options => options.UseSqlite(connection));

                using var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<FinanceDbContext>();
                dbContext.Database.EnsureCreated();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            connection.Dispose();
        }
    }
}

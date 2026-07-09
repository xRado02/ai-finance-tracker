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

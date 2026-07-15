using AiFinanceTracker.Contracts;
using AiFinanceTracker.Domain;
using AiFinanceTracker.Persistence;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AiFinanceTracker.Endpoints;

public static class FinanceEndpoints
{
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");

        api.MapGet("/categories", GetCategories);
        api.MapPost("/transactions", CreateTransaction);
        api.MapGet("/transactions", GetTransactions);
        api.MapDelete("/transactions/{id:guid}", DeleteTransaction);
        api.MapGet("/goals", GetGoals);
        api.MapPost("/goals", CreateGoal);
        api.MapGet("/dashboard/summary", GetDashboardSummary);

        return endpoints;
    }

    private static async Task<Ok<IReadOnlyList<CategoryResponse>>> GetCategories(FinanceDbContext dbContext)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponse(
                category.Id,
                category.Name,
                category.AppliesTo))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyList<CategoryResponse>>(categories);
    }

    private static async Task<Results<Created<TransactionResponse>, ValidationProblem, NotFound<ProblemDetails>>> CreateTransaction(
        CreateTransactionRequest request,
        FinanceDbContext dbContext)
    {
        var validationErrors = ValidateCreateTransactionRequest(request);
        if (validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var category = await dbContext.Categories
            .SingleOrDefaultAsync(category => category.Id == request.CategoryId);

        if (category is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Category not found",
                Detail = "The selected category does not exist.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!CanUseCategoryForTransactionType(category, request.Type))
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.CategoryId)] = ["The selected category does not apply to the transaction type."]
            });
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Type = request.Type,
            TransactionDate = request.TransactionDate,
            Description = request.Description,
            CategoryId = category.Id,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };

        dbContext.Transactions.Add(transaction);
        await dbContext.SaveChangesAsync();

        var response = ToTransactionResponse(transaction, category.Name);

        return TypedResults.Created($"/api/transactions/{transaction.Id}", response);
    }

    private static async Task<Results<Ok<IReadOnlyList<TransactionResponse>>, ValidationProblem>> GetTransactions(
        FinanceDbContext dbContext,
        int? limit)
    {
        const int defaultLimit = 50;
        const int maxLimit = 100;

        var requestedLimit = limit ?? defaultLimit;
        if (requestedLimit is < 1 or > maxLimit)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(limit)] = [$"Limit must be between 1 and {maxLimit}."]
            });
        }

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ThenByDescending(transaction => transaction.Id)
            .Include(transaction => transaction.Category)
            .Take(requestedLimit)
            .Select(transaction => new TransactionResponse(
                transaction.Id,
                transaction.Amount,
                transaction.Type,
                transaction.TransactionDate,
                transaction.Description,
                transaction.CategoryId,
                transaction.Category!.Name))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyList<TransactionResponse>>(transactions);
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>>> DeleteTransaction(
        Guid id,
        FinanceDbContext dbContext)
    {
        var transaction = await dbContext.Transactions
            .SingleOrDefaultAsync(item =>
                item.Id == id &&
                item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId);

        if (transaction is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Transaction not found",
                Detail = "The transaction does not exist in the default local profile.",
                Status = StatusCodes.Status404NotFound
            });
        }

        dbContext.Transactions.Remove(transaction);
        await dbContext.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    private static async Task<Ok<IReadOnlyList<GoalResponse>>> GetGoals(FinanceDbContext dbContext)
    {
        var currentAmount = await GetCurrentAmount(dbContext);
        var goals = await dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .OrderBy(goal => goal.Name)
            .Select(goal => new GoalResponse(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                currentAmount,
                CalculateProgressPercentage(currentAmount, goal.TargetAmount)))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyList<GoalResponse>>(goals);
    }

    private static async Task<Results<Created<GoalResponse>, ValidationProblem>> CreateGoal(
        CreateGoalRequest request,
        FinanceDbContext dbContext)
    {
        var validationErrors = ValidateCreateGoalRequest(request);
        if (validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            Name = request.Name!.Trim(),
            TargetAmount = request.TargetAmount,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };

        dbContext.Goals.Add(goal);
        await dbContext.SaveChangesAsync();

        var currentAmount = await GetCurrentAmount(dbContext);
        var response = ToGoalResponse(goal, currentAmount);

        return TypedResults.Created($"/api/goals/{goal.Id}", response);
    }

    private static async Task<Ok<DashboardSummaryResponse>> GetDashboardSummary(FinanceDbContext dbContext)
    {
        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .Select(transaction => new
            {
                transaction.Amount,
                transaction.Type,
                CategoryName = transaction.Category!.Name
            })
            .ToListAsync();

        var totalIncome = transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .Sum(transaction => transaction.Amount);
        var totalExpenses = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .Sum(transaction => transaction.Amount);
        var balance = totalIncome - totalExpenses;
        var currentAmount = Math.Max(0m, balance);
        var expenseCategories = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .GroupBy(transaction => transaction.CategoryName)
            .Select(group => new ExpenseCategorySummary(group.Key, group.Sum(item => item.Amount)))
            .OrderByDescending(category => category.Amount)
            .ThenBy(category => category.CategoryName)
            .ToList();
        var goals = await dbContext.Goals
            .AsNoTracking()
            .Where(goal => goal.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .OrderBy(goal => goal.Name)
            .Select(goal => new DashboardGoalSummary(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                currentAmount,
                CalculateProgressPercentage(currentAmount, goal.TargetAmount)))
            .ToListAsync();

        return TypedResults.Ok(new DashboardSummaryResponse(
            totalIncome,
            totalExpenses,
            balance,
            expenseCategories,
            goals));
    }

    private static Dictionary<string, string[]> ValidateCreateTransactionRequest(CreateTransactionRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (request.Amount <= 0)
        {
            errors[nameof(request.Amount)] = ["Amount must be greater than 0."];
        }

        if (!Enum.IsDefined(request.Type))
        {
            errors[nameof(request.Type)] = ["Type must be Income or Expense."];
        }

        if (request.TransactionDate == default)
        {
            errors[nameof(request.TransactionDate)] = ["TransactionDate is required."];
        }

        if (request.Description?.Length > 500)
        {
            errors[nameof(request.Description)] = ["Description must be 500 characters or fewer."];
        }

        return errors;
    }

    private static Dictionary<string, string[]> ValidateCreateGoalRequest(CreateGoalRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Goal name is required."];
        }
        else if (request.Name.Trim().Length > 120)
        {
            errors[nameof(request.Name)] = ["Goal name must be 120 characters or fewer."];
        }

        if (request.TargetAmount <= 0)
        {
            errors[nameof(request.TargetAmount)] = ["Target amount must be greater than 0."];
        }

        return errors;
    }

    private static async Task<decimal> GetCurrentAmount(FinanceDbContext dbContext)
    {
        var income = await dbContext.Transactions
            .Where(transaction =>
                transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId &&
                transaction.Type == TransactionType.Income)
            .SumAsync(transaction => transaction.Amount);
        var expenses = await dbContext.Transactions
            .Where(transaction =>
                transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId &&
                transaction.Type == TransactionType.Expense)
            .SumAsync(transaction => transaction.Amount);

        return Math.Max(0m, income - expenses);
    }

    private static decimal CalculateProgressPercentage(decimal currentAmount, decimal targetAmount)
    {
        if (targetAmount <= 0)
        {
            return 0m;
        }

        return Math.Round(Math.Min(currentAmount / targetAmount * 100m, 100m), 2);
    }

    private static bool CanUseCategoryForTransactionType(Category category, TransactionType transactionType)
    {
        return category.Id == FinanceDbContext.OtherCategoryId || category.AppliesTo == transactionType;
    }

    private static TransactionResponse ToTransactionResponse(Transaction transaction, string categoryName)
    {
        return new TransactionResponse(
            transaction.Id,
            transaction.Amount,
            transaction.Type,
            transaction.TransactionDate,
            transaction.Description,
            transaction.CategoryId,
            categoryName);
    }

    private static GoalResponse ToGoalResponse(Goal goal, decimal currentAmount)
    {
        return new GoalResponse(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            currentAmount,
            CalculateProgressPercentage(currentAmount, goal.TargetAmount));
    }
}

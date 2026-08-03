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
        api.MapGet("/profile/settings", GetProfileSettings);
        api.MapPatch("/profile/settings", UpdateProfileSettings);
        api.MapPost("/transactions", CreateTransaction);
        api.MapGet("/transactions", GetTransactions);
        api.MapDelete("/transactions/{id:guid}", DeleteTransaction);
        api.MapGet("/goals", GetGoals);
        api.MapPost("/goals", CreateGoal);
        api.MapGet("/goals/forecast", GetGoalForecast);
        api.MapGet("/dashboard/summary", GetDashboardSummary);
        api.MapGet("/dashboard/monthly-summary", GetMonthlySummary);
        api.MapGet("/recurring-transactions", GetRecurringTransactions);
        api.MapPost("/recurring-transactions", CreateRecurringTransaction);
        api.MapPatch("/recurring-transactions/{id:guid}/status", UpdateRecurringTransactionStatus);
        api.MapPost("/recurring-transactions/generate-current-month", GenerateCurrentMonthRecurringTransactions);

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

    private static async Task<Ok<ProfileSettingsResponse>> GetProfileSettings(FinanceDbContext dbContext)
    {
        var profile = await dbContext.LocalProfiles
            .AsNoTracking()
            .SingleAsync(item => item.Id == FinanceDbContext.DefaultLocalProfileId);

        return TypedResults.Ok(new ProfileSettingsResponse(profile.DisplayName, profile.InitialBalance));
    }

    private static async Task<Results<Ok<ProfileSettingsResponse>, ValidationProblem>> UpdateProfileSettings(
        UpdateProfileSettingsRequest request,
        FinanceDbContext dbContext)
    {
        const decimal maxBalance = 9999999999999999.99m;
        if (request.InitialBalance < -maxBalance || request.InitialBalance > maxBalance)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(request.InitialBalance)] = ["InitialBalance is outside the supported range."]
            });
        }

        var profile = await dbContext.LocalProfiles
            .SingleAsync(item => item.Id == FinanceDbContext.DefaultLocalProfileId);
        profile.InitialBalance = request.InitialBalance;
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(new ProfileSettingsResponse(profile.DisplayName, profile.InitialBalance));
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
        int? limit,
        int? year,
        int? month)
    {
        const int defaultLimit = 50;
        const int maxLimit = 100;

        var requestedLimit = limit ?? defaultLimit;
        var validationErrors = new Dictionary<string, string[]>();
        if (requestedLimit is < 1 or > maxLimit)
        {
            validationErrors[nameof(limit)] = [$"Limit must be between 1 and {maxLimit}."];
        }

        if (!TryGetMonthRange(year, month, out var monthStart, out var nextMonthStart, validationErrors) ||
            validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var query = dbContext.Transactions
            .AsNoTracking()
            .Where(transaction => transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .AsQueryable();

        if (monthStart is not null && nextMonthStart is not null)
        {
            query = query.Where(transaction =>
                transaction.TransactionDate >= monthStart.Value &&
                transaction.TransactionDate < nextMonthStart.Value);
        }

        var transactions = await query
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

    private static async Task<Ok<IReadOnlyList<GoalForecastResponse>>> GetGoalForecast(FinanceDbContext dbContext)
    {
        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(item => item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .Select(item => new
            {
                item.Amount,
                item.Type,
                item.TransactionDate
            })
            .ToListAsync();

        var totalIncome = transactions
            .Where(item => item.Type == TransactionType.Income)
            .Sum(item => item.Amount);
        var totalExpenses = transactions
            .Where(item => item.Type == TransactionType.Expense)
            .Sum(item => item.Amount);
        var initialBalance = await GetInitialBalance(dbContext);
        var currentAmount = Math.Max(0m, initialBalance + totalIncome - totalExpenses);
        var currentMonthStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var nextMonthStart = currentMonthStart.AddMonths(1);
        var currentMonthTransactions = transactions
            .Where(item => item.TransactionDate >= currentMonthStart && item.TransactionDate < nextMonthStart)
            .ToList();
        decimal? currentMonthSurplus = currentMonthTransactions.Count == 0
            ? null
            : currentMonthTransactions.Sum(item =>
                item.Type == TransactionType.Income ? item.Amount : -item.Amount);

        var goals = await dbContext.Goals
            .AsNoTracking()
            .Where(item => item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .OrderBy(item => item.Name)
            .ToListAsync();

        var forecasts = goals
            .Select(goal => BuildGoalForecastResponse(
                goal,
                currentAmount,
                currentMonthSurplus,
                currentMonthTransactions.Count))
            .ToList();

        return TypedResults.Ok<IReadOnlyList<GoalForecastResponse>>(forecasts);
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
        var initialBalance = await GetInitialBalance(dbContext);
        var balance = initialBalance + totalIncome - totalExpenses;
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
            initialBalance,
            totalIncome,
            totalExpenses,
            balance,
            expenseCategories,
            goals));
    }

    private static async Task<Results<Ok<MonthlySummaryResponse>, ValidationProblem>> GetMonthlySummary(
        FinanceDbContext dbContext,
        int? year,
        int? month)
    {
        var validationErrors = new Dictionary<string, string[]>();
        if (year is null && month is null)
        {
            validationErrors[nameof(year)] = ["Year is required."];
            validationErrors[nameof(month)] = ["Month is required."];
            return TypedResults.ValidationProblem(validationErrors);
        }

        if (!TryGetMonthRange(year, month, out var monthStart, out var nextMonthStart, validationErrors))
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var transactions = await dbContext.Transactions
            .AsNoTracking()
            .Where(transaction =>
                transaction.LocalProfileId == FinanceDbContext.DefaultLocalProfileId &&
                transaction.TransactionDate >= monthStart!.Value &&
                transaction.TransactionDate < nextMonthStart!.Value)
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
        var expenseCategories = transactions
            .Where(transaction => transaction.Type == TransactionType.Expense)
            .GroupBy(transaction => transaction.CategoryName)
            .Select(group => new ExpenseCategorySummary(group.Key, group.Sum(item => item.Amount)))
            .OrderByDescending(category => category.Amount)
            .ThenBy(category => category.CategoryName)
            .ToList();
        var incomeCategories = transactions
            .Where(transaction => transaction.Type == TransactionType.Income)
            .GroupBy(transaction => transaction.CategoryName)
            .Select(group => new IncomeCategorySummary(group.Key, group.Sum(item => item.Amount)))
            .OrderByDescending(category => category.Amount)
            .ThenBy(category => category.CategoryName)
            .ToList();

        return TypedResults.Ok(new MonthlySummaryResponse(
            year!.Value,
            month!.Value,
            totalIncome,
            totalExpenses,
            totalIncome - totalExpenses,
            expenseCategories,
            incomeCategories));
    }

    private static async Task<Ok<IReadOnlyList<RecurringTransactionResponse>>> GetRecurringTransactions(
        FinanceDbContext dbContext)
    {
        var recurringTransactions = await dbContext.RecurringTransactions
            .AsNoTracking()
            .Where(item => item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId)
            .OrderByDescending(item => item.IsActive)
            .ThenBy(item => item.Type)
            .ThenBy(item => item.Id)
            .Select(item => new RecurringTransactionResponse(
                item.Id,
                item.Amount,
                item.Type,
                item.CategoryId,
                item.Category!.Name,
                item.Description,
                item.IsActive))
            .ToListAsync();

        return TypedResults.Ok<IReadOnlyList<RecurringTransactionResponse>>(recurringTransactions);
    }

    private static async Task<Results<Created<RecurringTransactionResponse>, ValidationProblem, NotFound<ProblemDetails>>> CreateRecurringTransaction(
        CreateRecurringTransactionRequest request,
        FinanceDbContext dbContext)
    {
        var validationErrors = ValidateCreateRecurringTransactionRequest(request);
        if (validationErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var category = await dbContext.Categories
            .SingleOrDefaultAsync(item => item.Id == request.CategoryId);

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

        var recurring = new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Type = request.Type,
            CategoryId = category.Id,
            Description = request.Description,
            IsActive = request.IsActive,
            LocalProfileId = FinanceDbContext.DefaultLocalProfileId
        };

        dbContext.RecurringTransactions.Add(recurring);
        await dbContext.SaveChangesAsync();

        return TypedResults.Created(
            $"/api/recurring-transactions/{recurring.Id}",
            ToRecurringTransactionResponse(recurring, category.Name));
    }

    private static async Task<Results<Ok<RecurringTransactionResponse>, NotFound<ProblemDetails>>> UpdateRecurringTransactionStatus(
        Guid id,
        UpdateRecurringTransactionStatusRequest request,
        FinanceDbContext dbContext)
    {
        var recurring = await dbContext.RecurringTransactions
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item =>
                item.Id == id &&
                item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId);

        if (recurring is null)
        {
            return TypedResults.NotFound(new ProblemDetails
            {
                Title = "Recurring transaction not found",
                Detail = "The recurring transaction does not exist in the default local profile.",
                Status = StatusCodes.Status404NotFound
            });
        }

        recurring.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync();

        return TypedResults.Ok(ToRecurringTransactionResponse(recurring, recurring.Category!.Name));
    }

    private static async Task<Results<Ok<GenerateRecurringTransactionsResponse>, ValidationProblem>> GenerateCurrentMonthRecurringTransactions(
        FinanceDbContext dbContext,
        int? year,
        int? month)
    {
        var validationErrors = new Dictionary<string, string[]>();
        if (!TryGetMonthRange(year, month, out var requestedMonthStart, out var requestedNextMonthStart, validationErrors))
        {
            return TypedResults.ValidationProblem(validationErrors);
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        var monthStart = requestedMonthStart ?? new DateOnly(today.Year, today.Month, 1);
        var nextMonthStart = requestedNextMonthStart ?? monthStart.AddMonths(1);

        var recurringTransactions = await dbContext.RecurringTransactions
            .Where(item =>
                item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId &&
                item.IsActive)
            .Include(item => item.Category)
            .OrderBy(item => item.Id)
            .ToListAsync();

        var generatedRecurringIds = (await dbContext.Transactions
                .Where(item =>
                    item.LocalProfileId == FinanceDbContext.DefaultLocalProfileId &&
                    item.RecurringTransactionId != null &&
                    item.TransactionDate >= monthStart &&
                    item.TransactionDate < nextMonthStart)
                .Select(item => item.RecurringTransactionId!.Value)
                .ToListAsync())
            .ToHashSet();

        var generated = new List<Transaction>();
        var skippedCount = 0;
        foreach (var recurring in recurringTransactions)
        {
            if (generatedRecurringIds.Contains(recurring.Id))
            {
                skippedCount++;
                continue;
            }

            generated.Add(new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = recurring.Amount,
                Type = recurring.Type,
                TransactionDate = monthStart,
                Description = recurring.Description,
                CategoryId = recurring.CategoryId,
                RecurringTransactionId = recurring.Id,
                LocalProfileId = FinanceDbContext.DefaultLocalProfileId
            });
        }

        if (generated.Count > 0)
        {
            dbContext.Transactions.AddRange(generated);
            await dbContext.SaveChangesAsync();
        }

        var generatedResponses = generated
            .Select(item => ToTransactionResponse(
                item,
                recurringTransactions.Single(recurring => recurring.Id == item.RecurringTransactionId).Category!.Name))
            .ToList();

        return TypedResults.Ok(new GenerateRecurringTransactionsResponse(
            $"{monthStart.Year:D4}-{monthStart.Month:D2}",
            generated.Count,
            skippedCount,
            generatedResponses));
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

    private static Dictionary<string, string[]> ValidateCreateRecurringTransactionRequest(
        CreateRecurringTransactionRequest request)
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

        if (request.Description?.Length > 500)
        {
            errors[nameof(request.Description)] = ["Description must be 500 characters or fewer."];
        }

        return errors;
    }

    private static bool TryGetMonthRange(
        int? year,
        int? month,
        out DateOnly? monthStart,
        out DateOnly? nextMonthStart,
        Dictionary<string, string[]> errors)
    {
        monthStart = null;
        nextMonthStart = null;

        if (year is null && month is null)
        {
            return true;
        }

        if (year is null)
        {
            errors[nameof(year)] = ["Year is required when month is provided."];
        }
        else if (year is < 2000 or > 2100)
        {
            errors[nameof(year)] = ["Year must be between 2000 and 2100."];
        }

        if (month is null)
        {
            errors[nameof(month)] = ["Month is required when year is provided."];
        }
        else if (month is < 1 or > 12)
        {
            errors[nameof(month)] = ["Month must be between 1 and 12."];
        }

        if (errors.ContainsKey(nameof(year)) || errors.ContainsKey(nameof(month)))
        {
            return false;
        }

        monthStart = new DateOnly(year!.Value, month!.Value, 1);
        nextMonthStart = monthStart.Value.AddMonths(1);
        return true;
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

        var initialBalance = await GetInitialBalance(dbContext);
        return Math.Max(0m, initialBalance + income - expenses);
    }

    private static Task<decimal> GetInitialBalance(FinanceDbContext dbContext)
    {
        return dbContext.LocalProfiles
            .Where(item => item.Id == FinanceDbContext.DefaultLocalProfileId)
            .Select(item => item.InitialBalance)
            .SingleAsync();
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

    private static RecurringTransactionResponse ToRecurringTransactionResponse(
        RecurringTransaction recurring,
        string categoryName)
    {
        return new RecurringTransactionResponse(
            recurring.Id,
            recurring.Amount,
            recurring.Type,
            recurring.CategoryId,
            categoryName,
            recurring.Description,
            recurring.IsActive);
    }

    private static GoalForecastResponse BuildGoalForecastResponse(
        Goal goal,
        decimal currentAmount,
        decimal? currentMonthSurplus,
        int currentMonthTransactionCount)
    {
        var remainingAmount = Math.Max(0m, goal.TargetAmount - currentAmount);
        if (remainingAmount == 0m)
        {
            return new GoalForecastResponse(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                currentAmount,
                remainingAmount,
                currentMonthSurplus,
                null,
                null,
                GoalForecastStatus.Achieved);
        }

        if (currentMonthTransactionCount == 0)
        {
            return new GoalForecastResponse(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                currentAmount,
                remainingAmount,
                null,
                null,
                null,
                GoalForecastStatus.NoData);
        }

        if (currentMonthSurplus is null or <= 0m)
        {
            return new GoalForecastResponse(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                currentAmount,
                remainingAmount,
                currentMonthSurplus,
                null,
                null,
                GoalForecastStatus.NoPositiveSurplus);
        }

        var estimatedMonths = (int)Math.Ceiling(remainingAmount / currentMonthSurplus.Value);
        var estimatedDate = DateOnly.FromDateTime(DateTime.Today).AddMonths(estimatedMonths);

        return new GoalForecastResponse(
            goal.Id,
            goal.Name,
            goal.TargetAmount,
            currentAmount,
            remainingAmount,
            currentMonthSurplus,
            estimatedMonths,
            estimatedDate,
            GoalForecastStatus.Forecastable);
    }
}

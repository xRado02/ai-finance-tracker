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
}

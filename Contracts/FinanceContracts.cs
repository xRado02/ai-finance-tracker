using AiFinanceTracker.Domain;

namespace AiFinanceTracker.Contracts;

public sealed record CreateTransactionRequest(
    decimal Amount,
    TransactionType Type,
    DateOnly TransactionDate,
    string? Description,
    Guid CategoryId);

public sealed record TransactionResponse(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    DateOnly TransactionDate,
    string? Description,
    Guid CategoryId,
    string CategoryName);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    TransactionType? AppliesTo);

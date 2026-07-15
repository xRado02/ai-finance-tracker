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

public sealed record CreateGoalRequest(
    string? Name,
    decimal TargetAmount);

public sealed record GoalResponse(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage);

public sealed record ExpenseCategorySummary(
    string CategoryName,
    decimal Amount);

public sealed record DashboardGoalSummary(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage);

public sealed record DashboardSummaryResponse(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    IReadOnlyList<ExpenseCategorySummary> ExpenseCategories,
    IReadOnlyList<DashboardGoalSummary> Goals);

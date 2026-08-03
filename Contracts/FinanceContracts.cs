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

public sealed record ProfileSettingsResponse(
    string DisplayName,
    decimal InitialBalance);

public sealed record UpdateProfileSettingsRequest(decimal InitialBalance);

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

public sealed record IncomeCategorySummary(
    string CategoryName,
    decimal Amount);

public sealed record DashboardGoalSummary(
    Guid Id,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal ProgressPercentage);

public sealed record DashboardSummaryResponse(
    decimal InitialBalance,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    IReadOnlyList<ExpenseCategorySummary> ExpenseCategories,
    IReadOnlyList<DashboardGoalSummary> Goals);

public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    IReadOnlyList<ExpenseCategorySummary> ExpenseCategories,
    IReadOnlyList<IncomeCategorySummary> IncomeCategories);

public sealed record CreateRecurringTransactionRequest(
    decimal Amount,
    TransactionType Type,
    Guid CategoryId,
    string? Description,
    bool IsActive);

public sealed record UpdateRecurringTransactionStatusRequest(bool IsActive);

public sealed record RecurringTransactionResponse(
    Guid Id,
    decimal Amount,
    TransactionType Type,
    Guid CategoryId,
    string CategoryName,
    string? Description,
    bool IsActive);

public sealed record GenerateRecurringTransactionsResponse(
    string Month,
    int GeneratedCount,
    int SkippedCount,
    IReadOnlyList<TransactionResponse> Transactions);

public sealed record GoalForecastResponse(
    Guid GoalId,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal? CurrentMonthSurplus,
    int? EstimatedMonths,
    DateOnly? EstimatedDate,
    GoalForecastStatus Status);

namespace AiFinanceTracker.Domain;

public sealed class Transaction
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public DateOnly TransactionDate { get; set; }

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }

    public Guid LocalProfileId { get; set; }

    public LocalProfile? LocalProfile { get; set; }
}

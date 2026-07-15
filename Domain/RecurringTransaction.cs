namespace AiFinanceTracker.Domain;

public sealed class RecurringTransaction
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Category? Category { get; set; }

    public bool IsActive { get; set; }

    public Guid LocalProfileId { get; set; }

    public LocalProfile? LocalProfile { get; set; }

    public ICollection<Transaction> GeneratedTransactions { get; set; } = new List<Transaction>();
}

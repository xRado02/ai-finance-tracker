namespace AiFinanceTracker.Domain;

public sealed class Category
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public TransactionType? AppliesTo { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}

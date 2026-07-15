namespace AiFinanceTracker.Domain;

public sealed class LocalProfile
{
    public Guid Id { get; set; }

    public required string DisplayName { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    public ICollection<RecurringTransaction> RecurringTransactions { get; set; } = new List<RecurringTransaction>();
}

namespace AiFinanceTracker.Domain;

public sealed class Goal
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public decimal TargetAmount { get; set; }

    public Guid LocalProfileId { get; set; }

    public LocalProfile? LocalProfile { get; set; }
}

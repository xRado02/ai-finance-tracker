namespace AiFinanceTracker.Endpoints;

public static class FinanceEndpoints
{
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGroup("/api");

        return endpoints;
    }
}

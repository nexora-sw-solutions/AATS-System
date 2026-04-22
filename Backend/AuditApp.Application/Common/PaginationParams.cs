namespace AuditApp.Application.Common;

public class PaginationParams
{
    private int _page = 1;
    private int _limit = 10;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int Limit
    {
        get => _limit;
        set => _limit = value > 100 ? 100 : value < 1 ? 1 : value;
    }

    public string Sort { get; set; } = "created_at";
    public string Order { get; set; } = "desc";

    // Common filters used across many endpoints
    public Guid? BranchId { get; set; }
    public Guid? ClientId { get; set; }
    public string? PaymentStatus { get; set; }
    public string? Search { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
}

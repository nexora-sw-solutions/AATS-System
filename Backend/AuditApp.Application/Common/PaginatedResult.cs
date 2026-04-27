using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Common;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = [];
    public PaginationMeta Meta { get; set; } = new();

    public PaginatedResult() { }

    public PaginatedResult(List<T> items, int page, int limit, long total)
    {
        Items = items;
        Meta = new PaginationMeta
        {
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)limit)
        };
    }

    public static async Task<PaginatedResult<T>> CreateAsync(IQueryable<T> query, int page, int limit, CancellationToken cancellationToken = default)
    {
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items, page, limit, total);
    }
}

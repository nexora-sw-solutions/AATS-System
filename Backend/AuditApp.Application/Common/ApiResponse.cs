namespace AuditApp.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public PaginationMeta? Meta { get; set; }
    public ApiError? Error { get; set; }

    public static ApiResponse<T> Ok(T data)
        => new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string message, string code = "ERROR")
        => new() { Success = false, Error = new ApiError { Code = code, Message = message } };

    public static ApiResponse<T> FailWithDetails(string code, string message, List<FieldError>? details = null)
        => new() { Success = false, Error = new ApiError { Code = code, Message = message, Details = details } };
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public long Total { get; set; }
    public int TotalPages { get; set; }
}

public class ApiError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<FieldError>? Details { get; set; }
}

public class FieldError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

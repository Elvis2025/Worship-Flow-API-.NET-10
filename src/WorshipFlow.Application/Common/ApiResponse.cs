namespace WorshipFlow.Application.Common;

public sealed record ApiResponse<T>(bool Success, string Message, T? Data, IReadOnlyList<string> Errors)
{
    public static ApiResponse<T> Ok(T data, string message = "OK") => new(true, message, data, []);
    public static ApiResponse<T> Fail(string message, params string[] errors) => new(false, message, default, errors);
}

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Math.Max(PageSize, 1));
}

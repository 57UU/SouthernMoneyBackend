namespace SouthernMoneyBackend.Utils;

/// <summary>
/// 标准化的API响应格式
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? Message { get; set; }
    
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 成功响应
    /// </summary>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Timestamp = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 无数据的API响应格式
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// 成功响应（无数据）
    /// </summary>
    public static ApiResponse Ok()
    {
        return new ApiResponse
        {
            Success = true,
            Timestamp = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// 失败响应
    /// </summary>
    public new static ApiResponse Fail(string message)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// 分页响应格式
/// </summary>
public class PaginatedResponse<T> 
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int CurrentPage { get; set; }
    
    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// 总记录数
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 数据项列表
    /// </summary>
    public List<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 创建分页响应
    /// </summary>
    public static ApiResponse<PaginatedResponse<T>> Create(List<T> data, int page, int pageSize, int totalCount)
    {
        PaginatedResponse<T> payload = new PaginatedResponse<T>
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = data
        };
        return ApiResponse<PaginatedResponse<T>>.Ok(payload);
    }
}
    
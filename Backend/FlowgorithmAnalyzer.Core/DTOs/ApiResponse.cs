namespace FlowgorithmAnalyzer.Core.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = [];
}

public class ApiResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public List<string> Errors { get; set; } = [];
}

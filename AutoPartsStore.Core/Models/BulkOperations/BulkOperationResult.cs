// BulkOperationResult.cs
public class BulkOperationResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BulkOperationError> Errors { get; set; } = new();
}

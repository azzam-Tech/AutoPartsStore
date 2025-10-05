namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// مورد غير موجود (مثلاً: مدينة، حي، مستخدم...)
    /// </summary>
    public class NotFoundException : AppException
    {
        public string? ResourceType { get; }
        public object? ResourceId { get; }

        public NotFoundException(
            string message,
            string? resourceType = null,
            object? resourceId = null,
            string errorCode = "RESOURCE_NOT_FOUND")
            : base(message, errorCode)
        {
            ResourceType = resourceType;
            ResourceId = resourceId;
        }
    }
}

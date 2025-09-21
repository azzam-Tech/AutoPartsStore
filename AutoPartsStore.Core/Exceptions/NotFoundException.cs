namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// مورد غير موجود (مثلاً: مدينة، حي، مستخدم...)
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}

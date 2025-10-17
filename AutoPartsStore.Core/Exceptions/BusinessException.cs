namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بالـ Business Logic (مثلاً: حي مكرر داخل نفس المدينة)
    /// </summary>
    public class BusinessException : AppException
    {
        public BusinessException(
            string message,
            string errorCode = "BUSINESS_RULE_VIOLATION",
            Dictionary<string, object>? additionalData = null)
            : base(message, errorCode, additionalData)
        {
        }
    }
}

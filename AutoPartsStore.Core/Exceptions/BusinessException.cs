namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بالـ Business Logic (مثلاً: حي مكرر داخل نفس المدينة)
    /// </summary>
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}

namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بالصلاحيات (مثلاً: مستخدم عادي يحاول حذف مدينة)
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}

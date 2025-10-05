namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بالصلاحيات (مثلاً: مستخدم عادي يحاول حذف مدينة)
    /// </summary>
    public class ForbiddenException : AppException
    {
        public string? RequiredRole { get; }
        public string? RequiredPermission { get; }

        public ForbiddenException(
            string message = "ممنوع الوصول. ليس لديك الصلاحيات الكافية.",
            string errorCode = "FORBIDDEN",
            string? requiredRole = null,
            string? requiredPermission = null)
            : base(message, errorCode)
        {
            RequiredRole = requiredRole;
            RequiredPermission = requiredPermission;
        }
    }
}

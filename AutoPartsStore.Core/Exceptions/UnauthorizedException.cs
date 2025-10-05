namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بعملية غير مصرح بها (مثلاً: لم يسجل الدخول)
    /// </summary>
    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(
            string message = "غير مصرح. يرجى تسجيل الدخول.",
            string errorCode = "UNAUTHORIZED")
            : base(message, errorCode)
        {
        }
    }
}

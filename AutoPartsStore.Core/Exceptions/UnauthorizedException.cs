namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ متعلق بعملية غير مصرح بها (مثلاً: لم يسجل الدخول)
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
}

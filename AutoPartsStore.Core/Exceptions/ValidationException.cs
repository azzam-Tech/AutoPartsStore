namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// خطأ تحقق من البيانات (مثلاً: اسم مطلوب أو صيغة البريد غير صحيحة)
    /// </summary>
    public class ValidationException : AppException
    {
        public List<string> Errors { get; }
        public Dictionary<string, string[]>? ValidationErrors { get; }

        public ValidationException(
            string message,
            List<string>? errors = null,
            Dictionary<string, string[]>? validationErrors = null)
            : base(message, "VALIDATION_ERROR")
        {
            Errors = errors ?? new List<string>();
            ValidationErrors = validationErrors;
        }

        public ValidationException(
            string message,
            string errorCode,
            List<string>? errors = null)
            : base(message, errorCode)
        {
            Errors = errors ?? new List<string>();
        }
    }
}

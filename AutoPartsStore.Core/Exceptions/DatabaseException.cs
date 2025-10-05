namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// ��� �� ����� ��������
    /// </summary>
    public class DatabaseException : AppException
    {
        public string? SqlState { get; }
        public int? ErrorNumber { get; }

        public DatabaseException(
            string message,
            string errorCode = "DATABASE_ERROR",
            string? sqlState = null,
            int? errorNumber = null,
            Exception? innerException = null)
            : base(message, errorCode, null, innerException)
        {
            SqlState = sqlState;
            ErrorNumber = errorNumber;
        }
    }
}

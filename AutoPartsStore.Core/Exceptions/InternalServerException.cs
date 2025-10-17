namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// ��� ��� ����� �� �������
    /// </summary>
    public class InternalServerException : AppException
    {
        public InternalServerException(
            string message = "��� ��� ��� ����� �� ������. ���� �������� ������.",
            string errorCode = "INTERNAL_SERVER_ERROR",
            Exception? innerException = null)
            : base(message, errorCode, null, innerException)
        {
        }
    }
}

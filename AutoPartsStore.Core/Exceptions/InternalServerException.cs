namespace AutoPartsStore.Core.Exceptions
{
    /// <summary>
    /// ÎØÃ ÛíÑ ãÊæŞÚ İí ÇáÊØÈíŞ
    /// </summary>
    public class InternalServerException : AppException
    {
        public InternalServerException(
            string message = "ÍÏË ÎØÃ ÛíÑ ãÊæŞÚ İí ÇáÎÇÏã. íÑÌì ÇáãÍÇæáÉ áÇÍŞÇğ.",
            string errorCode = "INTERNAL_SERVER_ERROR",
            Exception? innerException = null)
            : base(message, errorCode, null, innerException)
        {
        }
    }
}

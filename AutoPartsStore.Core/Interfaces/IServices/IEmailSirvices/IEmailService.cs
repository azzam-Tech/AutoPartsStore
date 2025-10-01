namespace AutoPartsStore.Core.Interfaces.IServices.IEmailSirvices
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string toEmail, string code);
    }
}

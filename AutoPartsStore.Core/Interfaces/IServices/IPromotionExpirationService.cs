namespace AutoPartsStore.Core.Interfaces.IServices
{
    public interface IPromotionExpirationService
    {
        Task DeactivateExpiredPromotionsAsync();

    }
}

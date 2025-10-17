namespace AutoPartsStore.Core.Models.Payments.Moyasar
{
    public class MoyasarSettings
    {
        public string ApiKey { get; set; } = null!;
        public string PublishableKey { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://api.moyasar.com/v1";
        public string CallbackUrl { get; set; } = null!;
        public string Currency { get; set; } = "SAR";
        public bool TestMode { get; set; } = true;
    }
}

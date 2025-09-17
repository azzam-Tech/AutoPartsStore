using AutoPartsStore.Core.Interfaces;
using AutoPartsStore.Core.Interfaces.IServices;
using AutoPartsStore.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace AutoPartsStore.Infrastructure.Services
{
    public class PromotionExpirationService : IPromotionExpirationService
    {
        private readonly AppDbContext _context;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ICarPartRepository _carPartRepository;
        private readonly ILogger<PromotionExpirationService> _logger;

        public PromotionExpirationService(
            AppDbContext context,
            IPromotionRepository promotionRepository,
            ICarPartRepository carPartRepository,
            ILogger<PromotionExpirationService> logger)
        {
            _context = context;
            _promotionRepository = promotionRepository;
            _carPartRepository = carPartRepository;
            _logger = logger;
        }

        public async Task DeactivateExpiredPromotionsAsync()
        {
            _logger.LogInformation("بدء عملية التحقق من العروض المنتهية...");

            // 1. جلب كل العروض المنتهية والنشطة
            var expiredPromotions = await _promotionRepository.GetExpiredActivePromotionsAsync();

            if (!expiredPromotions.Any())
            {
                _logger.LogInformation("لا توجد عروض منتهية ليتم معالجتها.");
                return;
            }

            _logger.LogInformation($"تم العثور على {expiredPromotions.Count} عرض منتهي.");

            foreach (var promotion in expiredPromotions)
            {
                // 2. تحديث حالة العرض إلى غير نشط
                promotion.Deactivate();
                _logger.LogInformation($"تم تعطيل العرض: {promotion.PromotionName} (ID: {promotion.Id})");

                // 3. جلب كل المنتجات المرتبطة بهذا العرض
                var associatedParts = await _carPartRepository.GetPartsByPromotionIdAsync(promotion.Id);

                foreach (var part in associatedParts)
                {
                    // 4. إرجاع السعر الأصلي وحذف معرف العرض
                    part.UpdateFinalPrice(part.UnitPrice); // إرجاع السعر النهائي إلى السعر الأساسي
                    part.RemovePromotion(); // حذف معرف العرض من المنتج
                }
                _logger.LogInformation($"تم تحديث {associatedParts.Count} منتج مرتبط بالعرض.");
            }

            // 5. حفظ كل التغييرات في قاعدة البيانات مرة واحدة
            await _context.SaveChangesAsync();
            _logger.LogInformation("تم حفظ جميع التغييرات بنجاح.");
        }
    }
}

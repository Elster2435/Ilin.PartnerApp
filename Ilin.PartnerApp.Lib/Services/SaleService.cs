using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Lib.Services
{
    public class SaleService
    {
        private readonly IlinDbContext _context;

        public SaleService(IlinDbContext context)
        {
            _context = context;
        }

        public async Task<List<SaleListItemDTO>> GetSalesAsync()
        {
            return await _context.PartnerProductSales
                .AsNoTracking()
                .Include(x => x.Partner)
                .Include(x => x.Product)
                .OrderByDescending(x => x.SaleDate)
                .ThenByDescending(x => x.Id)
                .Select(x => new SaleListItemDTO
                {
                    Id = x.Id,
                    PartnerId = x.PartnerId,
                    PartnerName = x.Partner != null ? x.Partner.CompanyName : string.Empty,
                    ProductId = x.ProductId,
                    ProductName = x.Product != null ? x.Product.Name : string.Empty,
                    Quantity = x.Quantity,
                    SaleDate = x.SaleDate,
                    BasePrice = x.BasePrice,
                    DiscountPercent = x.DiscountPercent,
                    UnitPrice = x.UnitPrice,
                    TotalAmount = x.TotalAmount,
                    Comment = x.Comment
                })
                .ToListAsync();
        }

        public async Task<SaleEditModel?> GetSaleByIdAsync(int id)
        {
            return await _context.PartnerProductSales
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new SaleEditModel
                {
                    Id = x.Id,
                    PartnerId = x.PartnerId,
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    SaleDate = x.SaleDate,
                    Comment = x.Comment
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<SalePartnerLookupDTO>> GetPartnersForLookupAsync()
        {
            return await _context.Partners
                .AsNoTracking()
                .OrderBy(x => x.CompanyName)
                .Select(x => new SalePartnerLookupDTO
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName
                })
                .ToListAsync();
        }

        public async Task<List<SaleProductLookupDTO>> GetActiveProductsForLookupAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new SaleProductLookupDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Price = x.Price
                })
                .ToListAsync();
        }

        public async Task<SaleCalculationResultDTO> CalculateSaleAsync(int partnerId, int productId, int quantity, int? editingSaleId = null)
        {
            if (partnerId <= 0)
                throw new ArgumentException("Не выбран партнер.");

            if (productId <= 0)
                throw new ArgumentException("Не выбран товар.");

            if (quantity <= 0)
                throw new ArgumentException("Количество должно быть больше нуля.");

            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
                throw new InvalidOperationException("Товар не найден.");

            if (!product.IsActive)
                throw new InvalidOperationException("Нельзя оформить продажу по неактивному товару.");

            var totalQuantity = await _context.PartnerProductSales
                .AsNoTracking()
                .Where(x => x.PartnerId == partnerId && (!editingSaleId.HasValue || x.Id != editingSaleId.Value))
                .SumAsync(x => (int?)x.Quantity) ?? 0;

            var discountPercent = PartnerDiscountCalculator.Calculate(totalQuantity);

            var unitPrice = Math.Round(product.Price * (1m - discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
            var totalAmount = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero);

            return new SaleCalculationResultDTO
            {
                BasePrice = product.Price,
                DiscountPercent = discountPercent,
                UnitPrice = unitPrice,
                TotalAmount = totalAmount
            };
        }

        public async Task<int> AddSaleAsync(SaleEditModel model)
        {
            Validate(model);

            await EnsurePartnerExistsAsync(model.PartnerId);

            var calculation = await CalculateSaleAsync(model.PartnerId, model.ProductId, model.Quantity);

            var entity = new PartnerProductSale
            {
                PartnerId = model.PartnerId,
                ProductId = model.ProductId,
                Quantity = model.Quantity,
                SaleDate = model.SaleDate,
                BasePrice = calculation.BasePrice,
                DiscountPercent = calculation.DiscountPercent,
                UnitPrice = calculation.UnitPrice,
                Comment = NormalizeNullable(model.Comment)
            };

            _context.PartnerProductSales.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdateSaleAsync(SaleEditModel model)
        {
            Validate(model);

            var entity = await _context.PartnerProductSales
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null)
                throw new InvalidOperationException("Продажа не найдена.");

            await EnsurePartnerExistsAsync(model.PartnerId);

            var calculation = await CalculateSaleAsync(model.PartnerId, model.ProductId, model.Quantity, model.Id);

            entity.PartnerId = model.PartnerId;
            entity.ProductId = model.ProductId;
            entity.Quantity = model.Quantity;
            entity.SaleDate = model.SaleDate;
            entity.BasePrice = calculation.BasePrice;
            entity.DiscountPercent = calculation.DiscountPercent;
            entity.UnitPrice = calculation.UnitPrice;
            entity.Comment = NormalizeNullable(model.Comment);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSaleAsync(int id)
        {
            var entity = await _context.PartnerProductSales
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new InvalidOperationException("Продажа не найдена.");

            _context.PartnerProductSales.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private static void Validate(SaleEditModel model)
        {
            if (model.PartnerId <= 0)
                throw new ArgumentException("Выберите партнера.");

            if (model.ProductId <= 0)
                throw new ArgumentException("Выберите товар.");

            if (model.Quantity <= 0)
                throw new ArgumentException("Количество должно быть больше нуля.");
        }

        private async Task EnsurePartnerExistsAsync(int partnerId)
        {
            var exists = await _context.Partners
                .AsNoTracking()
                .AnyAsync(x => x.Id == partnerId);

            if (!exists)
                throw new InvalidOperationException("Выбранный партнер не существует.");
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}

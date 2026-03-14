using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Lib.Services
{
    public class PartnerService
    {
        private readonly IlinDbContext _context;

        public PartnerService(IlinDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartnerListItemDTO>> GetPartnersAsync()
        {
            var partners = await _context.Partners
                .AsNoTracking()
                .Include(x => x.PartnerType)
                .Include(x => x.PartnerProductSales)
                .OrderBy(x => x.CompanyName)
                .ToListAsync();

            return partners.Select(x =>
            {
                var totalQuantity = x.PartnerProductSales.Sum(s => s.Quantity);
                var discountPercent = PartnerDiscountCalculator.Calculate(totalQuantity);

                return new PartnerListItemDTO
                {
                    Id = x.Id,
                    CompanyName = x.CompanyName,
                    PartnerTypeName = x.PartnerType?.Name ?? string.Empty,
                    DirectorFullname = x.DirectorFullname,
                    Phone = x.Phone,
                    Email = x.Email,
                    Rating = x.Rating,
                    TotalQuantity = totalQuantity,
                    DiscountPercent = discountPercent
                };
            }).ToList();
        }

        public async Task<List<PartnerSaleHistoryItemDTO>> GetPartnerSalesHistoryAsync(int partnerId)
        {
            return await _context.PartnerProductSales
                .AsNoTracking()
                .Where(x => x.PartnerId == partnerId)
                .Include(x => x.Product)
                .OrderByDescending(x => x.SaleDate)
                .Select(x => new PartnerSaleHistoryItemDTO
                {
                    SaleId = x.Id,
                    ProductName = x.Product != null ? x.Product.Name : string.Empty,
                    Quantity = x.Quantity,
                    SaleDate = x.SaleDate,
                    UnitPrice = x.UnitPrice,
                    TotalAmount = x.TotalAmount,
                    Comment = x.Comment
                })
                .ToListAsync();
        }

        public async Task<PartnerEditModel?> GetPartnerByIdAsync(int partnerId)
        {
            return await _context.Partners
                .AsNoTracking()
                .Where(x => x.Id == partnerId)
                .Select(x => new PartnerEditModel
                {
                    Id = x.Id,
                    TypeId = x.TypeId,
                    CompanyName = x.CompanyName,
                    Address = x.Address,
                    Inn = x.Inn,
                    DirectorFullname = x.DirectorFullname,
                    Phone = x.Phone,
                    Email = x.Email,
                    Rating = x.Rating,
                    LogoPath = x.LogoPath,
                    SalesPlaces = x.SalesPlaces
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> AddPartnerAsync(PartnerEditModel model)
        {
            ValidatePartner(model);

            await EnsurePartnerTypeExistsAsync(model.TypeId);
            await EnsureUniquePartnerFieldsAsync(model);

            var partner = new Ilin.PartnerApp.Lib.Entities.Partner
            {
                TypeId = model.TypeId,
                CompanyName = model.CompanyName.Trim(),
                Address = model.Address.Trim(),
                Inn = NormalizeNullable(model.Inn),
                DirectorFullname = model.DirectorFullname.Trim(),
                Phone = model.Phone.Trim(),
                Email = model.Email.Trim(),
                Rating = model.Rating,
                LogoPath = NormalizeNullable(model.LogoPath),
                SalesPlaces = NormalizeNullable(model.SalesPlaces),
                CreatedAt = DateTime.UtcNow
            };

            _context.Partners.Add(partner);
            await _context.SaveChangesAsync();

            return partner.Id;
        }

        public async Task UpdatePartnerAsync(PartnerEditModel model)
        {
            ValidatePartner(model);

            var partner = await _context.Partners
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (partner == null)
                throw new InvalidOperationException("Партнер не найден.");

            await EnsurePartnerTypeExistsAsync(model.TypeId);
            await EnsureUniquePartnerFieldsAsync(model);

            partner.TypeId = model.TypeId;
            partner.CompanyName = model.CompanyName.Trim();
            partner.Address = model.Address.Trim();
            partner.Inn = NormalizeNullable(model.Inn);
            partner.DirectorFullname = model.DirectorFullname.Trim();
            partner.Phone = model.Phone.Trim();
            partner.Email = model.Email.Trim();
            partner.Rating = model.Rating;
            partner.LogoPath = NormalizeNullable(model.LogoPath);
            partner.SalesPlaces = NormalizeNullable(model.SalesPlaces);

            await _context.SaveChangesAsync();
        }

        public async Task DeletePartnerAsync(int partnerId)
        {
            var partner = await _context.Partners
                .FirstOrDefaultAsync(x => x.Id == partnerId);

            if (partner == null)
                throw new InvalidOperationException("Партнер не найден.");

            var hasSales = await _context.PartnerProductSales
                .AnyAsync(x => x.PartnerId == partnerId);

            if (hasSales)
                throw new InvalidOperationException("Нельзя удалить партнера, у которого есть история продаж.");

            _context.Partners.Remove(partner);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PartnerType>> GetPartnerTypesAsync()
        {
            return await _context.PartnerTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        private static void ValidatePartner(PartnerEditModel model)
        {
            if (model.TypeId <= 0)
                throw new ArgumentException("Не выбран тип партнера.");

            if (string.IsNullOrWhiteSpace(model.CompanyName))
                throw new ArgumentException("Введите наименование компании.");

            if (string.IsNullOrWhiteSpace(model.Address))
                throw new ArgumentException("Введите адрес.");

            if (string.IsNullOrWhiteSpace(model.DirectorFullname))
                throw new ArgumentException("Введите ФИО директора.");

            if (string.IsNullOrWhiteSpace(model.Phone))
                throw new ArgumentException("Введите телефон.");

            if (string.IsNullOrWhiteSpace(model.Email))
                throw new ArgumentException("Введите email.");

            if (model.Rating < 0)
                throw new ArgumentException("Рейтинг не может быть отрицательным.");

            if (!string.IsNullOrWhiteSpace(model.Inn))
            {
                var inn = model.Inn.Trim();

                if (inn.Length != 10 && inn.Length != 12)
                    throw new ArgumentException("ИНН должен содержать 10 или 12 символов.");
            }
        }

        private async Task EnsurePartnerTypeExistsAsync(int typeId)
        {
            var exists = await _context.PartnerTypes
                .AsNoTracking()
                .AnyAsync(x => x.Id == typeId);

            if (!exists)
                throw new InvalidOperationException("Выбранный тип партнера не существует.");
        }

        private async Task EnsureUniquePartnerFieldsAsync(PartnerEditModel model)
        {
            var normalizedEmail = model.Email.Trim();
            var normalizedPhone = model.Phone.Trim();
            var normalizedInn = NormalizeNullable(model.Inn);

            var emailExists = await _context.Partners
                .AsNoTracking()
                .AnyAsync(x => x.Email == normalizedEmail && x.Id != model.Id);

            if (emailExists)
                throw new InvalidOperationException("Партнер с таким email уже существует.");

            var phoneExists = await _context.Partners
                .AsNoTracking()
                .AnyAsync(x => x.Phone == normalizedPhone && x.Id != model.Id);

            if (phoneExists)
                throw new InvalidOperationException("Партнер с таким phone уже существует.");

            if (!string.IsNullOrWhiteSpace(normalizedInn))
            {
                var innExists = await _context.Partners
                    .AsNoTracking()
                    .AnyAsync(x => x.Inn == normalizedInn && x.Id != model.Id);

                if (innExists)
                    throw new InvalidOperationException("Партнер с таким ИНН уже существует.");
            }
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}

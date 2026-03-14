using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Lib.Services
{
    public class PartnerTypeService
    {
        private readonly IlinDbContext _context;

        public PartnerTypeService(IlinDbContext context)
        {
            _context = context;
        }

        public async Task<List<PartnerTypeListItemDTO>> GetPartnerTypesAsync()
        {
            return await _context.PartnerTypes
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new PartnerTypeListItemDTO
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();
        }

        public async Task<PartnerTypeEditModel?> GetPartnerTypeByIdAsync(int id)
        {
            return await _context.PartnerTypes
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new PartnerTypeEditModel
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> AddPartnerTypeAsync(PartnerTypeEditModel model)
        {
            Validate(model);
            await EnsureUniqueNameAsync(model);

            var entity = new PartnerType
            {
                Name = model.Name.Trim()
            };

            _context.PartnerTypes.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdatePartnerTypeAsync(PartnerTypeEditModel model)
        {
            Validate(model);

            var entity = await _context.PartnerTypes
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null)
                throw new InvalidOperationException("Тип партнера не найден.");

            await EnsureUniqueNameAsync(model);

            entity.Name = model.Name.Trim();

            await _context.SaveChangesAsync();
        }

        public async Task DeletePartnerTypeAsync(int id)
        {
            var entity = await _context.PartnerTypes
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new InvalidOperationException("Тип партнера не найден.");

            var isUsed = await _context.Partners
                .AsNoTracking()
                .AnyAsync(x => x.TypeId == id);

            if (isUsed)
                throw new InvalidOperationException("Нельзя удалить тип партнера, который используется в карточках партнеров.");

            _context.PartnerTypes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private static void Validate(PartnerTypeEditModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Введите название типа партнера.");
        }

        private async Task EnsureUniqueNameAsync(PartnerTypeEditModel model)
        {
            var normalizedName = model.Name.Trim();

            var exists = await _context.PartnerTypes
                .AsNoTracking()
                .AnyAsync(x => x.Name == normalizedName && x.Id != model.Id);

            if (exists)
                throw new InvalidOperationException("Тип партнера с таким названием уже существует.");
        }
    }
}

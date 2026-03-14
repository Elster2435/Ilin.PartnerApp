using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Models;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Lib.Services
{
    public class ProductService
    {
        private readonly IlinDbContext _context;

        public ProductService(IlinDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductListItemDTO>> GetProductsAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new ProductListItemDTO
                {
                    Id = x.Id,
                    Article = x.Article,
                    ProductType = x.ProductType,
                    Name = x.Name,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .ToListAsync();
        }

        public async Task<ProductEditModel?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new ProductEditModel
                {
                    Id = x.Id,
                    Article = x.Article,
                    ProductType = x.ProductType,
                    Name = x.Name,
                    Price = x.Price,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> AddProductAsync(ProductEditModel model)
        {
            Validate(model);
            await EnsureUniqueArticleAsync(model);

            var entity = new Product
            {
                Article = model.Article.Trim(),
                ProductType = NormalizeNullable(model.ProductType),
                Name = model.Name.Trim(),
                Price = model.Price,
                IsActive = model.IsActive
            };

            _context.Products.Add(entity);
            await _context.SaveChangesAsync();

            return entity.Id;
        }

        public async Task UpdateProductAsync(ProductEditModel model)
        {
            Validate(model);

            var entity = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (entity == null)
                throw new InvalidOperationException("Товар не найден.");

            await EnsureUniqueArticleAsync(model);

            entity.Article = model.Article.Trim();
            entity.ProductType = NormalizeNullable(model.ProductType);
            entity.Name = model.Name.Trim();
            entity.Price = model.Price;
            entity.IsActive = model.IsActive;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var entity = await _context.Products
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
                throw new InvalidOperationException("Товар не найден.");

            var isUsed = await _context.PartnerProductSales
                .AsNoTracking()
                .AnyAsync(x => x.ProductId == id);

            if (isUsed)
                throw new InvalidOperationException("Нельзя удалить товар, который используется в продажах.");

            _context.Products.Remove(entity);
            await _context.SaveChangesAsync();
        }

        private static void Validate(ProductEditModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Article))
                throw new ArgumentException("Введите артикул.");

            if (string.IsNullOrWhiteSpace(model.Name))
                throw new ArgumentException("Введите название товара.");

            if (model.Price < 0)
                throw new ArgumentException("Цена не может быть отрицательной.");
        }

        private async Task EnsureUniqueArticleAsync(ProductEditModel model)
        {
            var article = model.Article.Trim();

            var exists = await _context.Products
                .AsNoTracking()
                .AnyAsync(x => x.Article == article && x.Id != model.Id);

            if (exists)
                throw new InvalidOperationException("Товар с таким артикулом уже существует.");
        }

        private static string? NormalizeNullable(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}

using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Services;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Tests
{
    [TestClass]
    public class SaleServiceTests
    {
        private IlinDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<IlinDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new IlinDbContext(options);
        }

        [TestMethod]
        public async Task CalculateSaleAsync_ShouldApply_0PercentDiscount_ForNewPartner()
        {
            using var context = CreateContext(nameof(CalculateSaleAsync_ShouldApply_0PercentDiscount_ForNewPartner));

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                Id = 1,
                TypeId = 1,
                CompanyName = "ООО Альфа",
                Address = "Адрес",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "a@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Article = "A-001",
                Name = "Товар 1",
                Price = 1000,
                IsActive = true
            });

            await context.SaveChangesAsync();

            var service = new SaleService(context);

            var result = await service.CalculateSaleAsync(1, 1, 2);

            Assert.AreEqual(1000, result.BasePrice);
            Assert.AreEqual(0, result.DiscountPercent);
            Assert.AreEqual(1000, result.UnitPrice);
            Assert.AreEqual(2000, result.TotalAmount);
        }

        [TestMethod]
        public async Task CalculateSaleAsync_ShouldApply_5PercentDiscount_WhenPartnerHas10000Sales()
        {
            using var context = CreateContext(nameof(CalculateSaleAsync_ShouldApply_5PercentDiscount_WhenPartnerHas10000Sales));

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                Id = 1,
                TypeId = 1,
                CompanyName = "ООО Бета",
                Address = "Адрес",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "b@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Article = "A-001",
                Name = "Товар 1",
                Price = 1000,
                IsActive = true
            });

            context.PartnerProductSales.Add(new PartnerProductSale
            {
                Id = 1,
                PartnerId = 1,
                ProductId = 1,
                Quantity = 10000,
                SaleDate = DateOnly.FromDateTime(DateTime.Today),
                BasePrice = 1000,
                DiscountPercent = 5,
                UnitPrice = 950,
                Comment = null
            });

            await context.SaveChangesAsync();

            var service = new SaleService(context);

            var result = await service.CalculateSaleAsync(1, 1, 2);

            Assert.AreEqual(1000, result.BasePrice);
            Assert.AreEqual(5, result.DiscountPercent);
            Assert.AreEqual(950, result.UnitPrice);
            Assert.AreEqual(1900, result.TotalAmount);
        }

        [TestMethod]
        public async Task CalculateSaleAsync_ShouldThrowException_ForInactiveProduct()
        {
            using var context = CreateContext(nameof(CalculateSaleAsync_ShouldThrowException_ForInactiveProduct));

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                Id = 1,
                TypeId = 1,
                CompanyName = "ООО Гамма",
                Address = "Адрес",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "c@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Article = "A-001",
                Name = "Товар 1",
                Price = 1000,
                IsActive = false
            });

            await context.SaveChangesAsync();

            var service = new SaleService(context);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.CalculateSaleAsync(1, 1, 1);
            });

            Assert.AreEqual("Нельзя оформить продажу по неактивному товару.", ex.Message);
        }

        [TestMethod]
        public async Task AddSaleAsync_ShouldSaveSnapshotPriceAndDiscount()
        {
            using var context = CreateContext(nameof(AddSaleAsync_ShouldSaveSnapshotPriceAndDiscount));

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                Id = 1,
                TypeId = 1,
                CompanyName = "ООО Дельта",
                Address = "Адрес",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "d@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            context.Products.Add(new Product
            {
                Id = 1,
                Article = "A-001",
                Name = "Товар 1",
                Price = 2000,
                IsActive = true
            });

            context.PartnerProductSales.Add(new PartnerProductSale
            {
                Id = 1,
                PartnerId = 1,
                ProductId = 1,
                Quantity = 10000,
                SaleDate = DateOnly.FromDateTime(DateTime.Today),
                BasePrice = 2000,
                DiscountPercent = 5,
                UnitPrice = 1900
            });

            await context.SaveChangesAsync();

            var service = new SaleService(context);

            await service.AddSaleAsync(new Lib.Models.SaleEditModel
            {
                PartnerId = 1,
                ProductId = 1,
                Quantity = 3,
                SaleDate = DateOnly.FromDateTime(DateTime.Today),
                Comment = "Тест"
            });

            var savedSale = await context.PartnerProductSales
                .OrderByDescending(x => x.Id)
                .FirstAsync();

            Assert.AreEqual(2000, savedSale.BasePrice);
            Assert.AreEqual(5, savedSale.DiscountPercent);
            Assert.AreEqual(1900, savedSale.UnitPrice);
        }

        [TestMethod]
        public async Task AddSaleAsync_ShouldKeepSavedPrice_WhenProductPriceChangesLater()
        {
            using var context = CreateContext(nameof(AddSaleAsync_ShouldKeepSavedPrice_WhenProductPriceChangesLater));

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                Id = 1,
                TypeId = 1,
                CompanyName = "ООО Эпсилон",
                Address = "Адрес",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "e@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            var product = new Product
            {
                Id = 1,
                Article = "A-001",
                Name = "Товар 1",
                Price = 1000,
                IsActive = true
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            var service = new SaleService(context);

            var saleId = await service.AddSaleAsync(new Lib.Models.SaleEditModel
            {
                PartnerId = 1,
                ProductId = 1,
                Quantity = 2,
                SaleDate = DateOnly.FromDateTime(DateTime.Today),
                Comment = null
            });

            product.Price = 5000;
            await context.SaveChangesAsync();

            var savedSale = await context.PartnerProductSales.FirstAsync(x => x.Id == saleId);

            Assert.AreEqual(1000, savedSale.BasePrice);
            Assert.AreEqual(1000, savedSale.UnitPrice);
        }
    }
}

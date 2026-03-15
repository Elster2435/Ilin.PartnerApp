using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Entities;
using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using Microsoft.EntityFrameworkCore;

namespace Ilin.PartnerApp.Tests
{
    [TestClass]
    public class PartnerServiceTests
    {
        private IlinDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<IlinDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;

            return new IlinDbContext(options);
        }

        private async Task<int> AddPartnerTypeAsync(IlinDbContext context, string name = "Розничный магазин")
        {
            var partnerType = new PartnerType
            {
                Name = name
            };

            context.PartnerTypes.Add(partnerType);
            await context.SaveChangesAsync();

            return partnerType.Id;
        }

        [TestMethod]
        public async Task AddPartnerAsync_ShouldCreatePartner_WhenModelIsValid()
        {
            using var context = CreateContext(nameof(AddPartnerAsync_ShouldCreatePartner_WhenModelIsValid));
            var typeId = await AddPartnerTypeAsync(context);

            var service = new PartnerService(context);

            var model = new PartnerEditModel
            {
                TypeId = typeId,
                CompanyName = "ООО Альфа",
                Address = "г. Москва, ул. Тестовая, д. 1",
                Inn = "7701234567",
                DirectorFullname = "Иванов Иван Иванович",
                Phone = "+7(900)123-45-67",
                Email = "alpha@test.com",
                Rating = 5,
                LogoPath = null,
                SalesPlaces = "Розничный магазин"
            };

            var newId = await service.AddPartnerAsync(model);

            var savedPartner = await context.Partners.FirstAsync(x => x.Id == newId);

            Assert.AreEqual("ООО Альфа", savedPartner.CompanyName);
            Assert.AreEqual(typeId, savedPartner.TypeId);
            Assert.AreEqual("7701234567", savedPartner.Inn);
            Assert.AreEqual("alpha@test.com", savedPartner.Email);
            Assert.AreEqual(5, savedPartner.Rating);
        }

        [TestMethod]
        public async Task AddPartnerAsync_ShouldThrow_WhenRatingIsNegative()
        {
            using var context = CreateContext(nameof(AddPartnerAsync_ShouldThrow_WhenRatingIsNegative));
            var typeId = await AddPartnerTypeAsync(context);

            var service = new PartnerService(context);

            var model = new PartnerEditModel
            {
                TypeId = typeId,
                CompanyName = "ООО Бета",
                Address = "г. Казань, ул. Пример, д. 2",
                Inn = "1651234567",
                DirectorFullname = "Петров Петр Петрович",
                Phone = "+7(900)222-33-44",
                Email = "beta@test.com",
                Rating = -1
            };

            var ex = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await service.AddPartnerAsync(model);
            });

            Assert.AreEqual("Рейтинг не может быть отрицательным.", ex.Message);
        }

        [TestMethod]
        public async Task AddPartnerAsync_ShouldThrow_WhenPartnerTypeDoesNotExist()
        {
            using var context = CreateContext(nameof(AddPartnerAsync_ShouldThrow_WhenPartnerTypeDoesNotExist));
            var service = new PartnerService(context);

            var model = new PartnerEditModel
            {
                TypeId = 999,
                CompanyName = "ООО Гамма",
                Address = "г. Самара, ул. Новая, д. 3",
                Inn = "6311234567",
                DirectorFullname = "Сидоров Сергей Сергеевич",
                Phone = "+7(900)333-44-55",
                Email = "gamma@test.com",
                Rating = 3
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.AddPartnerAsync(model);
            });

            Assert.AreEqual("Выбранный тип партнера не существует.", ex.Message);
        }

        [TestMethod]
        public async Task AddPartnerAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            using var context = CreateContext(nameof(AddPartnerAsync_ShouldThrow_WhenEmailAlreadyExists));
            var typeId = await AddPartnerTypeAsync(context);

            context.Partners.Add(new PartnerApp.Lib.Entities.Partner
            {
                TypeId = typeId,
                CompanyName = "ООО Первый",
                Address = "Адрес 1",
                Inn = "7700000001",
                DirectorFullname = "Директор 1",
                Phone = "111",
                Email = "one@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var service = new PartnerService(context);

            var model = new PartnerEditModel
            {
                TypeId = typeId,
                CompanyName = "ООО Второй",
                Address = "Адрес 2",
                Inn = "7700000002",
                DirectorFullname = "Директор 2",
                Phone = "222",
                Email = "one@test.com",
                Rating = 2
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.AddPartnerAsync(model);
            });

            Assert.AreEqual("Партнер с таким email уже существует.", ex.Message);
        }

        [TestMethod]
        public async Task DeletePartnerAsync_ShouldRemovePartner_WhenPartnerHasNoSales()
        {
            using var context = CreateContext(nameof(DeletePartnerAsync_ShouldRemovePartner_WhenPartnerHasNoSales));
            var typeId = await AddPartnerTypeAsync(context);

            var partner = new PartnerApp.Lib.Entities.Partner
            {
                TypeId = typeId,
                CompanyName = "ООО Удаляемый",
                Address = "Адрес",
                Inn = "7709999999",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "delete@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Partners.Add(partner);
            await context.SaveChangesAsync();

            var service = new PartnerService(context);

            await service.DeletePartnerAsync(partner.Id);

            var exists = await context.Partners.AnyAsync(x => x.Id == partner.Id);

            Assert.IsFalse(exists);
        }

        [TestMethod]
        public async Task DeletePartnerAsync_ShouldThrow_WhenPartnerHasSales()
        {
            using var context = CreateContext(nameof(DeletePartnerAsync_ShouldThrow_WhenPartnerHasSales));
            var typeId = await AddPartnerTypeAsync(context);

            var partner = new PartnerApp.Lib.Entities.Partner
            {
                TypeId = typeId,
                CompanyName = "ООО С продажами",
                Address = "Адрес",
                Inn = "7708888888",
                DirectorFullname = "Директор",
                Phone = "123",
                Email = "sales@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            };

            var product = new Product
            {
                Article = "P-001",
                Name = "Товар 1",
                Price = 1000m,
                IsActive = true
            };

            context.Partners.Add(partner);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            context.PartnerProductSales.Add(new PartnerProductSale
            {
                PartnerId = partner.Id,
                ProductId = product.Id,
                Quantity = 10,
                SaleDate = DateOnly.FromDateTime(DateTime.Today),
                BasePrice = 1000m,
                DiscountPercent = 0,
                UnitPrice = 1000m,
                Comment = null
            });

            await context.SaveChangesAsync();

            var service = new PartnerService(context);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await service.DeletePartnerAsync(partner.Id);
            });

            Assert.AreEqual("Нельзя удалить партнера, у которого есть история продаж.", ex.Message);
        }

        [TestMethod]
        public async Task UpdatePartnerAsync_ShouldChangePartnerData()
        {
            using var context = CreateContext(nameof(UpdatePartnerAsync_ShouldChangePartnerData));
            var typeId = await AddPartnerTypeAsync(context, "Оптовый магазин");

            var partner = new PartnerApp.Lib.Entities.Partner
            {
                TypeId = typeId,
                CompanyName = "ООО Старое имя",
                Address = "Старый адрес",
                Inn = "7707777777",
                DirectorFullname = "Старый директор",
                Phone = "111",
                Email = "old@test.com",
                Rating = 1,
                CreatedAt = DateTime.UtcNow
            };

            context.Partners.Add(partner);
            await context.SaveChangesAsync();

            var service = new PartnerService(context);

            var model = new PartnerEditModel
            {
                Id = partner.Id,
                TypeId = typeId,
                CompanyName = "ООО Новое имя",
                Address = "Новый адрес",
                Inn = "7707777777",
                DirectorFullname = "Новый директор",
                Phone = "222",
                Email = "new@test.com",
                Rating = 10,
                LogoPath = "logo.png",
                SalesPlaces = "Онлайн"
            };

            await service.UpdatePartnerAsync(model);

            var updatedPartner = await context.Partners.FirstAsync(x => x.Id == partner.Id);

            Assert.AreEqual("ООО Новое имя", updatedPartner.CompanyName);
            Assert.AreEqual("Новый адрес", updatedPartner.Address);
            Assert.AreEqual("Новый директор", updatedPartner.DirectorFullname);
            Assert.AreEqual("222", updatedPartner.Phone);
            Assert.AreEqual("new@test.com", updatedPartner.Email);
            Assert.AreEqual(10, updatedPartner.Rating);
            Assert.AreEqual("logo.png", updatedPartner.LogoPath);
            Assert.AreEqual("Онлайн", updatedPartner.SalesPlaces);
        }
    }
}

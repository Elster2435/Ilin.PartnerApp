using Ilin.PartnerApp.Lib.Services;

namespace Ilin.PartnerApp.Tests
{
    [TestClass]
    public class PartnerDiscountCalculatorTests
    {
        [TestMethod]
        public void Calculate_ShouldReturn_0_WhenQuantityIsLessThan10000()
        {
            var result = PartnerDiscountCalculator.Calculate(9999);

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void Calculate_ShouldReturn_5_WhenQuantityIs10000()
        {
            var result = PartnerDiscountCalculator.Calculate(10000);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void Calculate_ShouldReturn_5_WhenQuantityIs49999()
        {
            var result = PartnerDiscountCalculator.Calculate(49999);

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public void Calculate_ShouldReturn_10_WhenQuantityIs50000()
        {
            var result = PartnerDiscountCalculator.Calculate(50000);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void Calculate_ShouldReturn_10_WhenQuantityIs299999()
        {
            var result = PartnerDiscountCalculator.Calculate(299999);

            Assert.AreEqual(10, result);
        }

        [TestMethod]
        public void Calculate_ShouldReturn_15_WhenQuantityIs300000()
        {
            var result = PartnerDiscountCalculator.Calculate(300000);

            Assert.AreEqual(15, result);
        }

        [TestMethod]
        public void Calculate_ShouldThrowException_WhenQuantityIsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                PartnerDiscountCalculator.Calculate(-1);
            });
        }
    }
}

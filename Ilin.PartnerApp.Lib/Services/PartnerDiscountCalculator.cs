namespace Ilin.PartnerApp.Lib.Services
{
    public static class PartnerDiscountCalculator
    {
        public static int Calculate(int totalQuantity)
        {
            if (totalQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(totalQuantity), "Количество не может быть отрицательным.");

            if (totalQuantity < 10000)
                return 0;

            if (totalQuantity < 50000)
                return 5;

            if (totalQuantity < 300000)
                return 10;

            return 15;
        }
    }
}

using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Globalization;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class SaleEditWindow : Window
    {
        private readonly SaleService _saleService;
        private readonly int? _id;
        private bool _isLoading;

        public bool IsSaved { get; private set; }

        public SaleEditWindow(SaleService saleService, int? id = null)
        {
            InitializeComponent();

            _saleService = saleService;
            _id = id;

            Loaded += SaleEditWindow_Loaded;
        }

        private async void SaleEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;

                await LoadLookupsAsync();

                if (_id.HasValue)
                {
                    Title = "Редактирование продажи";
                    StatusTextBlock.Text = "Режим редактирования";

                    var model = await _saleService.GetSaleByIdAsync(_id.Value);

                    if (model == null)
                        throw new InvalidOperationException("Продажа не найдена.");

                    PartnerComboBox.SelectedValue = model.PartnerId;
                    ProductComboBox.SelectedValue = model.ProductId;
                    QuantityTextBox.Text = model.Quantity.ToString();
                    SaleDatePicker.SelectedDate = model.SaleDate.ToDateTime(TimeOnly.MinValue);
                    CommentTextBox.Text = model.Comment;
                }
                else
                {
                    Title = "Добавление продажи";
                    StatusTextBlock.Text = "Режим добавления";
                    SaleDatePicker.SelectedDate = DateTime.Today;
                }

                _isLoading = false;
                await RefreshCalculationAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при открытии формы:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadLookupsAsync()
        {
            var partners = await _saleService.GetPartnersForLookupAsync();
            PartnerComboBox.ItemsSource = partners;

            var products = await _saleService.GetActiveProductsForLookupAsync();
            ProductComboBox.ItemsSource = products;
        }

        private async void InputControl_Changed(object sender, EventArgs e)
        {
            if (_isLoading)
                return;

            await RefreshCalculationAsync();
        }

        private async Task RefreshCalculationAsync()
        {
            try
            {
                BasePriceTextBlock.Text = "0.00";
                DiscountTextBlock.Text = "0%";
                UnitPriceTextBlock.Text = "0.00";
                TotalAmountTextBlock.Text = "0.00";

                if (PartnerComboBox.SelectedValue == null || ProductComboBox.SelectedValue == null)
                    return;

                if (!int.TryParse(QuantityTextBox.Text.Trim(), out int quantity))
                    return;

                if (quantity <= 0)
                    return;

                var result = await _saleService.CalculateSaleAsync(
                    (int)PartnerComboBox.SelectedValue,
                    (int)ProductComboBox.SelectedValue,
                    quantity,
                    _id);

                BasePriceTextBlock.Text = result.BasePrice.ToString("N2", CultureInfo.InvariantCulture);
                DiscountTextBlock.Text = $"{result.DiscountPercent}%";
                UnitPriceTextBlock.Text = result.UnitPrice.ToString("N2", CultureInfo.InvariantCulture);
                TotalAmountTextBlock.Text = result.TotalAmount.ToString("N2", CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;

                if (PartnerComboBox.SelectedValue == null)
                    throw new ArgumentException("Выберите партнера.");

                if (ProductComboBox.SelectedValue == null)
                    throw new ArgumentException("Выберите товар.");

                if (!int.TryParse(QuantityTextBox.Text.Trim(), out int quantity))
                    throw new ArgumentException("Количество должно быть целым числом.");

                if (quantity <= 0)
                    throw new ArgumentException("Количество должно быть больше нуля.");

                if (SaleDatePicker.SelectedDate == null)
                    throw new ArgumentException("Выберите дату продажи.");

                var model = new SaleEditModel
                {
                    Id = _id ?? 0,
                    PartnerId = (int)PartnerComboBox.SelectedValue,
                    ProductId = (int)ProductComboBox.SelectedValue,
                    Quantity = quantity,
                    SaleDate = DateOnly.FromDateTime(SaleDatePicker.SelectedDate.Value),
                    Comment = CommentTextBox.Text
                };

                if (_id.HasValue)
                {
                    await _saleService.UpdateSaleAsync(model);
                }
                else
                {
                    await _saleService.AddSaleAsync(model);
                }

                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            finally
            {
                SaveButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

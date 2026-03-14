using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class SalesWindow : Window
    {
        private readonly IlinDbContext _context;
        private readonly SaleService _saleService;

        public SalesWindow()
        {
            InitializeComponent();

            _context = new IlinDbContext();
            _saleService = new SaleService(_context);

            Loaded += SalesWindow_Loaded;
        }

        private async void SalesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _saleService.GetSalesAsync();
                SalesDataGrid.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке продаж:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new SaleEditWindow(_saleService)
            {
                Owner = this
            };

            var result = window.ShowDialog();

            if (result == true && window.IsSaved)
            {
                await LoadDataAsync();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is not SaleListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите продажу.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var window = new SaleEditWindow(_saleService, selectedItem.Id)
            {
                Owner = this
            };

            var result = window.ShowDialog();

            if (result == true && window.IsSaved)
            {
                await LoadDataAsync();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SalesDataGrid.SelectedItem is not SaleListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите продажу.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = MessageBox.Show(
                $"Удалить продажу №{selectedItem.Id}?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _saleService.DeleteSaleAsync(selectedItem.Id);
                await LoadDataAsync();

                MessageBox.Show(
                    "Продажа успешно удалена.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}

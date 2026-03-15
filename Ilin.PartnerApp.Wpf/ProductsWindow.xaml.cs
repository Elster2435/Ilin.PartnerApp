using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class ProductsWindow : Window
    {
        private readonly IlinDbContext _context;
        private readonly ProductService _productService;

        public ProductsWindow()
        {
            InitializeComponent();

            _context = new IlinDbContext();
            _productService = new ProductService(_context);

            Loaded += ProductsWindow_Loaded;
        }

        private async void ProductsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _productService.GetProductsAsync();
                ProductsDataGrid.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке товаров:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductEditWindow(_productService)
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
            if (ProductsDataGrid.SelectedItem is not ProductListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите товар.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var window = new ProductEditWindow(_productService, selectedItem.Id)
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
            if (ProductsDataGrid.SelectedItem is not ProductListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите товар.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = MessageBox.Show(
                $"Удалить товар \"{selectedItem.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _productService.DeleteProductAsync(selectedItem.Id);
                await LoadDataAsync();

                MessageBox.Show(
                    "Товар успешно удален.",
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

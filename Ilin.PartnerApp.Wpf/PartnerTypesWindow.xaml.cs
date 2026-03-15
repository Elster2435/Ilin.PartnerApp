using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class PartnerTypesWindow : Window
    {
        private readonly IlinDbContext _context;
        private readonly PartnerTypeService _partnerTypeService;

        public PartnerTypesWindow()
        {
            InitializeComponent();

            _context = new IlinDbContext();
            _partnerTypeService = new PartnerTypeService(_context);

            Loaded += PartnerTypesWindow_Loaded;
        }

        private async void PartnerTypesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var data = await _partnerTypeService.GetPartnerTypesAsync();
                PartnerTypesDataGrid.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке типов партнеров:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new PartnerTypeEditWindow(_partnerTypeService)
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
            if (PartnerTypesDataGrid.SelectedItem is not PartnerTypeListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите тип партнера.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var window = new PartnerTypeEditWindow(_partnerTypeService, selectedItem.Id)
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
            if (PartnerTypesDataGrid.SelectedItem is not PartnerTypeListItemDTO selectedItem)
            {
                MessageBox.Show(
                    "Сначала выберите тип партнера.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = MessageBox.Show(
                $"Удалить тип партнера \"{selectedItem.Name}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _partnerTypeService.DeletePartnerTypeAsync(selectedItem.Id);
                await LoadDataAsync();

                MessageBox.Show(
                    "Тип партнера успешно удален.",
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

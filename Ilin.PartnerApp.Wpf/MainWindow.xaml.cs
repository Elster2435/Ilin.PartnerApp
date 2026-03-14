using Ilin.Partner.Lib.Context;
using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly IlinDbContext _context;
        private readonly PartnerService _partnerService;

        public MainWindow()
        {
            InitializeComponent();

            _context = new IlinDbContext();
            _partnerService = new PartnerService(_context);

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadPartnersAsync();
        }

        private async Task LoadPartnersAsync()
        {
            try
            {
                var partners = await _partnerService.GetPartnersAsync();
                PartnersListBox.ItemsSource = partners;

                SalesHistoryDataGrid.ItemsSource = null;
                SelectedPartnerTextBlock.Text = "Партнер не выбран";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке партнеров:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async Task LoadPartnerSalesHistoryAsync(int partnerId, string companyName)
        {
            try
            {
                var history = await _partnerService.GetPartnerSalesHistoryAsync(partnerId);
                SalesHistoryDataGrid.ItemsSource = history;
                SelectedPartnerTextBlock.Text = $"История продаж: {companyName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при загрузке истории продаж:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void PartnersListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is PartnerListItemDTO selectedPartner)
            {
                await LoadPartnerSalesHistoryAsync(selectedPartner.Id, selectedPartner.CompanyName);
            }
            else
            {
                SalesHistoryDataGrid.ItemsSource = null;
                SelectedPartnerTextBlock.Text = "Партнер не выбран";
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadPartnersAsync();
        }

        private async void AddPartnerButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new PartnerEditWindow(_partnerService)
            {
                Owner = this
            };

            var result = editWindow.ShowDialog();

            if (result == true && editWindow.IsSaved)
            {
                await LoadPartnersAsync();
            }
        }

        private async void EditPartnerButton_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is not PartnerListItemDTO selectedPartner)
            {
                MessageBox.Show(
                    "Сначала выберите партнера.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var editWindow = new PartnerEditWindow(_partnerService, selectedPartner.Id)
            {
                Owner = this
            };

            var result = editWindow.ShowDialog();

            if (result == true && editWindow.IsSaved)
            {
                await LoadPartnersAsync();
            }
        }

        private async void DeletePartnerButton_Click(object sender, RoutedEventArgs e)
        {
            if (PartnersListBox.SelectedItem is not PartnerListItemDTO selectedPartner)
            {
                MessageBox.Show(
                    "Сначала выберите партнера.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var result = MessageBox.Show(
                $"Удалить партнера \"{selectedPartner.CompanyName}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _partnerService.DeletePartnerAsync(selectedPartner.Id);
                await LoadPartnersAsync();

                MessageBox.Show(
                    "Партнер успешно удален.",
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

        private void PartnerTypesButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new PartnerTypesWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ProductsWindow
            {
                Owner = this
            };

            window.ShowDialog();
        }

        private void SalesButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
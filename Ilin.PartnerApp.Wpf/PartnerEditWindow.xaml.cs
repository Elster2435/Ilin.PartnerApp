using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class PartnerEditWindow : Window
    {
        private readonly PartnerService _partnerService;
        private readonly int? _partnerId;

        public bool IsSaved { get; private set; }

        public PartnerEditWindow(PartnerService partnerService, int? partnerId = null)
        {
            InitializeComponent();

            _partnerService = partnerService;
            _partnerId = partnerId;

            Loaded += PartnerEditWindow_Loaded;
        }

        private async void PartnerEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadPartnerTypesAsync();

                if (_partnerId.HasValue)
                {
                    Title = "Редактирование партнера";
                    await LoadPartnerAsync(_partnerId.Value);
                    StatusTextBlock.Text = "Режим редактирования";
                }
                else
                {
                    Title = "Добавление партнера";
                    StatusTextBlock.Text = "Режим добавления";
                }
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
        }

        private async Task LoadPartnerTypesAsync()
        {
            var partnerTypes = await _partnerService.GetPartnerTypesAsync();
            PartnerTypeComboBox.ItemsSource = partnerTypes;
        }

        private async Task LoadPartnerAsync(int partnerId)
        {
            var partner = await _partnerService.GetPartnerByIdAsync(partnerId);

            if (partner == null)
                throw new InvalidOperationException("Партнер не найден.");

            PartnerTypeComboBox.SelectedValue = partner.TypeId;
            CompanyNameTextBox.Text = partner.CompanyName;
            AddressTextBox.Text = partner.Address;
            InnTextBox.Text = partner.Inn;
            DirectorFullnameTextBox.Text = partner.DirectorFullname;
            PhoneTextBox.Text = partner.Phone;
            EmailTextBox.Text = partner.Email;
            RatingTextBox.Text = partner.Rating.ToString();
            LogoPathTextBox.Text = partner.LogoPath;
            SalesPlacesTextBox.Text = partner.SalesPlaces;
        }

        private PartnerEditModel BuildModelFromForm()
        {
            if (PartnerTypeComboBox.SelectedValue == null)
                throw new ArgumentException("Выберите тип партнера.");

            if (!int.TryParse(RatingTextBox.Text.Trim(), out int rating))
                throw new ArgumentException("Рейтинг должен быть целым числом.");

            return new PartnerEditModel
            {
                Id = _partnerId ?? 0,
                TypeId = (int)PartnerTypeComboBox.SelectedValue,
                CompanyName = CompanyNameTextBox.Text,
                Address = AddressTextBox.Text,
                Inn = InnTextBox.Text,
                DirectorFullname = DirectorFullnameTextBox.Text,
                Phone = PhoneTextBox.Text,
                Email = EmailTextBox.Text,
                Rating = rating,
                LogoPath = LogoPathTextBox.Text,
                SalesPlaces = SalesPlacesTextBox.Text
            };
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;

                var model = BuildModelFromForm();

                if (_partnerId.HasValue)
                {
                    await _partnerService.UpdatePartnerAsync(model);
                }
                else
                {
                    await _partnerService.AddPartnerAsync(model);
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

using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class PartnerTypeEditWindow : Window
    {
        private readonly PartnerTypeService _partnerTypeService;
        private readonly int? _id;

        public bool IsSaved { get; private set; }

        public PartnerTypeEditWindow(PartnerTypeService partnerTypeService, int? id = null)
        {
            InitializeComponent();

            _partnerTypeService = partnerTypeService;
            _id = id;

            Loaded += PartnerTypeEditWindow_Loaded;
        }

        private async void PartnerTypeEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_id.HasValue)
                {
                    Title = "Редактирование типа партнера";
                    StatusTextBlock.Text = "Режим редактирования";

                    var model = await _partnerTypeService.GetPartnerTypeByIdAsync(_id.Value);

                    if (model == null)
                        throw new InvalidOperationException("Тип партнера не найден.");

                    NameTextBox.Text = model.Name;
                }
                else
                {
                    Title = "Добавление типа партнера";
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

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveButton.IsEnabled = false;
                CancelButton.IsEnabled = false;

                var model = new PartnerTypeEditModel
                {
                    Id = _id ?? 0,
                    Name = NameTextBox.Text
                };

                if (_id.HasValue)
                {
                    await _partnerTypeService.UpdatePartnerTypeAsync(model);
                }
                else
                {
                    await _partnerTypeService.AddPartnerTypeAsync(model);
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

using Ilin.PartnerApp.Lib.Models;
using Ilin.PartnerApp.Lib.Services;
using System.Globalization;
using System.Windows;

namespace Ilin.PartnerApp.Wpf
{
    public partial class ProductEditWindow : Window
    {
        private readonly ProductService _productService;
        private readonly int? _id;

        public bool IsSaved { get; private set; }

        public ProductEditWindow(ProductService productService, int? id = null)
        {
            InitializeComponent();

            _productService = productService;
            _id = id;

            Loaded += ProductEditWindow_Loaded;
        }

        private async void ProductEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_id.HasValue)
                {
                    Title = "Редактирование товара";
                    StatusTextBlock.Text = "Режим редактирования";

                    var model = await _productService.GetProductByIdAsync(_id.Value);

                    if (model == null)
                        throw new InvalidOperationException("Товар не найден.");

                    ArticleTextBox.Text = model.Article;
                    ProductTypeTextBox.Text = model.ProductType;
                    NameTextBox.Text = model.Name;
                    PriceTextBox.Text = model.Price.ToString(CultureInfo.InvariantCulture);
                    IsActiveCheckBox.IsChecked = model.IsActive;
                }
                else
                {
                    Title = "Добавление товара";
                    StatusTextBlock.Text = "Режим добавления";
                    IsActiveCheckBox.IsChecked = true;
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

                if (!decimal.TryParse(PriceTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) &&
                    !decimal.TryParse(PriceTextBox.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out price))
                {
                    throw new ArgumentException("Цена должна быть числом.");
                }

                var model = new ProductEditModel
                {
                    Id = _id ?? 0,
                    Article = ArticleTextBox.Text,
                    ProductType = ProductTypeTextBox.Text,
                    Name = NameTextBox.Text,
                    Price = price,
                    IsActive = IsActiveCheckBox.IsChecked ?? false
                };

                if (_id.HasValue)
                {
                    await _productService.UpdateProductAsync(model);
                }
                else
                {
                    await _productService.AddProductAsync(model);
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

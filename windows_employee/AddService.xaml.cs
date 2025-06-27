using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Provider.windows_employee
{
    public partial class AddService : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public AddService()
        {
            InitializeComponent();
            checkTheme(Manager.theme);
        }

        private void checkTheme(string mainTheme)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (mainTheme == "light")
            {
                theme.SetBaseTheme(Theme.Light);

                closeImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/close-icon.png"));
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);

                mainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494949"));
                windowChromeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494949"));

                closeImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/close-icon-white.png"));
            }

            paletteHelper.SetTheme(theme);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this);

            if (mousePosition.Y <= 26)
            {
                DragMove();
            }
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;

            var clip = new RectangleGeometry
            {
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft,
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight)
            };
            border.Clip = clip;

            border.SizeChanged += (s, args) =>
            {
                clip.Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight);
            };
        }

        private CancellationTokenSource _cancellationTokenSource;
        private async void ShowAndHideError(string errorName)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var showError = (Storyboard)FindResource("ShowError");
            var hideError = (Storyboard)FindResource("HideError");

            errorText.Text = errorName;
            errorLabel.Visibility = Visibility.Visible;
            showError.Begin(errorLabel);

            try
            {
                await Task.Delay(2500, cancellationToken);
                hideError.Begin(errorLabel);

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = _cancellationTokenSource.Token;
                await Task.Delay(2500, cancellationToken);
                errorLabel.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void previewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsDigit(c) && c != '-' && c != '.')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void typeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tarifDescriptionGrid == null || descriptionTB == null || addressCB == null)
                return;

            switch (typeCB.SelectedIndex)
            {
                case 0:
                    tarifDescriptionGrid.Visibility = Visibility.Visible;
                    descriptionTB.Visibility = Visibility.Collapsed;

                    addressCB.SelectedIndex = 0;
                    addressCB.IsEnabled = false;

                    break;

                case 1:
                case 2:
                    tarifDescriptionGrid.Visibility = Visibility.Collapsed;
                    descriptionTB.Visibility = Visibility.Visible;

                    addressCB.SelectedIndex = 1;
                    addressCB.IsEnabled = false;

                    costTB.Text = string.Empty;
                    costTB.IsEnabled = true;
                    break;

                case 4:
                    tarifDescriptionGrid.Visibility = Visibility.Collapsed;
                    descriptionTB.Visibility = Visibility.Visible;

                    addressCB.SelectedIndex = 0;
                    addressCB.IsEnabled = true;

                    costTB.Text = "0";
                    costTB.IsEnabled = false;
                    break;

                default:
                    tarifDescriptionGrid.Visibility = Visibility.Collapsed;
                    descriptionTB.Visibility = Visibility.Visible;

                    addressCB.SelectedIndex = 0;
                    addressCB.IsEnabled = true;

                    costTB.Text = string.Empty;
                    costTB.IsEnabled = true;
                    break;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (typeCB.SelectedIndex == 0)
            {
                saveTarif();
            }
            else
            {
                saveService();
            }
        }

        private void saveTarif()
        {
            if (nameTB.Text == string.Empty || costTB.Text == string.Empty || internetTB.Text == string.Empty || phoneTB.Text == string.Empty || smsTB.Text == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить\nвсе поля!");
                return;
            }

            decimal cost;
            string costText = costTB.Text.Trim();

            if (!Regex.IsMatch(costText, @"^\d+(\.\d{1,2})?$"))
            {
                ShowAndHideError("Неверно указана\nстоимость!");
                return;
            }

            try
            {
                cost = Convert.ToDecimal(costTB.Text, CultureInfo.InvariantCulture);
            }
            catch
            {
                ShowAndHideError("Неверно указана\nстоимость!");
                return;
            }

            int internet;
            int phone;
            int sms;

            try
            {
                internet = Convert.ToInt32(internetTB.Text);
                phone = Convert.ToInt32(phoneTB.Text);
                sms = Convert.ToInt32(smsTB.Text);
            }
            catch
            {
                ShowAndHideError("Неверно указаны\nпараметры!");
                return;
            }

            if (internet < -1 || phone < -1 || sms < -1)
            {
                ShowAndHideError("Неверно указаны\nпараметры!");
                return;
            }

            string description = $"{(internet >= 0 ? internet + " Гб интернета" : "Безлимитный интернет")}, {(phone >= 0 ? phone + " минут бесплатных звонков" : "безлимитные бесплатные звонки")}, " +
                                 $"{(sms >= 0 ? sms + " SMS" : "безлимитные SMS")}";

            if (!Manager.dataBase.completeCommand($"INSERT INTO `services`(`service_name`, `availability`, `type_id`, `cost`, `description`, `internet`, `phone`, `sms`, `need_an_address`) " +
                                                  $"VALUES ('{nameTB.Text}', True, 1, {cost.ToString().Replace(',', '.')}, '{description}', {internet}, {phone}, {sms}, False)"))
            {
                Close(); 
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Добавил услугу ({nameTB.Text})", DateTime.Now);

            Close();
        }

        private void saveService()
        {
            if (nameTB.Text == string.Empty || costTB.Text == string.Empty || descriptionTB.Text == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить\nвсе поля!");
                return;
            }

            decimal cost;
            string costText = costTB.Text.Trim();

            if (!Regex.IsMatch(costText, @"^\d+(\.\d{1,2})?$"))
            {
                ShowAndHideError("Неверно указана\nстоимость!");
                return;
            }

            try
            {
                cost = Convert.ToDecimal(costTB.Text, CultureInfo.InvariantCulture);
            }
            catch
            {
                ShowAndHideError("Неверно указана\nстоимость!");
                return;
            }

            int type = typeCB.SelectedIndex + 1;
            bool needAddress = addressCB.SelectedIndex != 0;

            if (!Manager.dataBase.completeCommand($"INSERT INTO `services`(`service_name`, `availability`, `type_id`, `cost`, `description`, `internet`, `phone`, `sms`, `need_an_address`) " +
                                                  $"VALUES ('{nameTB.Text}', True, {type}, {cost.ToString().Replace(',', '.')}, '{descriptionTB.Text}', 0, 0, 0, {needAddress})"))
            {
                Close();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Добавил услугу ({nameTB.Text})", DateTime.Now);

            Close();
        }
    }
}

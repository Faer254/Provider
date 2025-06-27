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
    public partial class EditService : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private List<string> _currentService;
        public EditService(List<string> service)
        {
            _currentService = service;
            InitializeComponent();
            checkTheme(Manager.theme);

            nameTB.Text = _currentService[1];
            costTB.Text = _currentService[4];

            if (_currentService[2] == "True")
            {
                availabilityCB.SelectedIndex = 0;
                _currentService[2] = availabilityCB.Text;
            }
            else
            {
                availabilityCB.SelectedIndex = 1;
                _currentService[2] = availabilityCB.Text;
            }

            if (_currentService[3] == "1")
            {
                tarifDescriptionGrid.Visibility = Visibility.Visible;
                descriptionTB.Visibility = Visibility.Collapsed;

                internetTB.Text = _currentService[6];
                phoneTB.Text = _currentService[7];
                smsTB.Text = _currentService[8];
            }
            else if (_currentService[3] == "5")
            {
                tarifDescriptionGrid.Visibility = Visibility.Collapsed;
                descriptionTB.Visibility = Visibility.Visible;

                costTB.Text = _currentService[4];
                costTB.IsEnabled = false;

                descriptionTB.Text = _currentService[5];
            }
            else
            {
                tarifDescriptionGrid.Visibility = Visibility.Collapsed;
                descriptionTB.Visibility = Visibility.Visible;

                costTB.Text = _currentService[4];
                costTB.IsEnabled = true;

                descriptionTB.Text = _currentService[5];
            }
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

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentService[3] == "1")
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
            if (nameTB.Text == _currentService[1] && costTB.Text == _currentService[4] && availabilityCB.Text == _currentService[2] &&
                internetTB.Text == _currentService[6] && phoneTB.Text == _currentService[7] && smsTB.Text == _currentService[8])
            {
                Close();
                return;
            }

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

            bool availability = availabilityCB.SelectedIndex == 0;

            if (!Manager.dataBase.completeCommand($"UPDATE `services` SET " +
                                                  $"`service_name` = '{nameTB.Text}', " +
                                                  $"`availability` = {availability}, " +
                                                  $"`cost` = {cost.ToString().Replace(',', '.')}, " +
                                                  $"`description` = '{description}', " +
                                                  $"`internet` = {internet}, " +
                                                  $"`phone` = {phone}, " +
                                                  $"`sms` = {sms} " +
                                                  $"WHERE `id` = {_currentService[0]}"))
            {
                Close();
                return;
            }

            var changes = new List<string>();
            var oldValues = _currentService;
            var newValues = new[] { 
                "",
                nameTB.Text,
                availabilityCB.Text,
                "",
                costTB.Text,
                descriptionTB.Text,
                internetTB.Text,
                phoneTB.Text,
                smsTB.Text,
                "",
                ""
            };

            var fieldNames = new[] { 
                "",
                "Название",
                "Доступность",
                "",
                "Стоимость",
                "",
                "Гб",
                "Мин", 
                "SMS",
                "",
                ""
            };

            for (int i = 1; i < oldValues.Count; i++)
            {
                if (string.IsNullOrEmpty(fieldNames[i])) 
                    continue;

                if (oldValues[i] != newValues[i])
                {
                    changes.Add($"{fieldNames[i]}: \"{oldValues[i]}\" -> \"{newValues[i]}\"");
                }
            }

            string actionMessage = $"Изменил услугу id {_currentService[0]} ({_currentService[1]}). {string.Join(". ", changes)}.";
    
            Manager.dataBase.addEmployeeAction(
                (uint)Manager.user[0],
                actionMessage,
                DateTime.Now
            );

            Close();
        }

        private void saveService()
        {
            if (nameTB.Text == _currentService[1] && costTB.Text == _currentService[4] && availabilityCB.Text == _currentService[2] && descriptionTB.Text == _currentService[5])
            {
                Close();
                return;
            }

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

            bool availability = availabilityCB.SelectedIndex == 0;

            if (!Manager.dataBase.completeCommand($"UPDATE `services` SET " +
                                                  $"`service_name` = '{nameTB.Text}', " +
                                                  $"`availability` = {availability}, " +
                                                  $"`cost` = {cost.ToString().Replace(',', '.')}, " +
                                                  $"`description` = '{descriptionTB.Text}' " +
                                                  $"WHERE `id` = {_currentService[0]}"
                                                ))
            {
                Close();
                return;
            }

            var changes = new List<string>();
            var oldValues = _currentService;
            var newValues = new[] {
                "",
                nameTB.Text,
                availabilityCB.Text,
                "",
                costTB.Text,
                descriptionTB.Text,
                "",
                "",
                "",
                "",
                ""
            };

            var fieldNames = new[] {
                "",
                "Название",
                "Доступность",
                "",
                "Стоимость",
                "Описание",
                "",
                "",
                "",
                "",
                ""
            };

            for (int i = 1; i < oldValues.Count; i++)
            {
                if (string.IsNullOrEmpty(fieldNames[i]))
                    continue;

                if (oldValues[i] != newValues[i])
                {
                    changes.Add($"{fieldNames[i]}: \"{oldValues[i]}\" -> \"{newValues[i]}\"");
                }
            }

            string actionMessage = $"Изменил услугу id {_currentService[0]} ({_currentService[1]}). {string.Join(". ", changes)}.";

            Manager.dataBase.addEmployeeAction(
                (uint)Manager.user[0],
                actionMessage,
                DateTime.Now
            );

            Close();
        }
    }
}


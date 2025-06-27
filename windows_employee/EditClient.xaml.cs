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
    public partial class EditClient : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private List<string> _currentClient;
        public EditClient(List<string> client)
        {
            InitializeComponent();
            _currentClient = client;
            checkTheme(Manager.theme);

            phoneTB.Text = _currentClient[1];
            emailTB.Text = _currentClient[2];
            fullNameTB.Text = _currentClient[4];
            
            if (_currentClient[2] != string.Empty)
            {
                emailTB.Visibility = Visibility.Visible;
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

        private void fullNameTB_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            foreach (char c in e.Text)
            {
                if (!char.IsLetter(c) && c != ' ')
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        private void fullNameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            while (fullNameTB.Text.Count(c => c == ' ') >= 3)
            {
                fullNameTB.Text = fullNameTB.Text.Remove(fullNameTB.Text.Length - 1);
                fullNameTB.CaretIndex = fullNameTB.Text.Length;
            }
        }

        private readonly string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            double accountWallet;
            try
            {
                accountWallet = double.Parse(walletTB.Text, CultureInfo.InvariantCulture);
            }
            catch
            {
                ShowAndHideError("Неверно указана\nсумма пополнения!");
                return;
            }

            if (phoneTB.Text == _currentClient[1] && emailTB.Text == _currentClient[2] && fullNameTB.Text == _currentClient[4] && accountWallet == 0)
            {
                Close();
                return;
            }

            if (accountWallet < 0)
            {
                ShowAndHideError("Неверно указана\nсумма пополнения!");
                return;
            }

            if (phoneTB.Text.Contains('_'))
            {
                ShowAndHideError("Необходимо указать\nномер телефона!");
                return;
            }

            string isPhoneRegistered = Manager.dataBase.isRecordExists("client", "phone_number", phoneTB.Text);

            if (isPhoneRegistered == "true" && phoneTB.Text != _currentClient[1])
            {
                ShowAndHideError("Данный номер телефона\nуже занят!");
                return;
            }

            if (isPhoneRegistered == "error")
            {
                Close();
                return;
            }

            if ((!Regex.Match(emailTB.Text, pattern, RegexOptions.IgnoreCase).Success || emailTB.Text.Contains(' ')) && emailTB.Text != _currentClient[2])
            {
                ShowAndHideError("Некорректный e-mail\nадрес!");
                return;
            }

            string isEmailRegistered = Manager.dataBase.isRecordExists("employee", "email", emailTB.Text);

            if (isEmailRegistered == "true" && emailTB.Text != _currentClient[2])
            {
                ShowAndHideError("Данная электронная почта\nуже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                Close();
                return;
            }

            isEmailRegistered = Manager.dataBase.isRecordExists("client", "email", emailTB.Text);

            if (isEmailRegistered == "true" && emailTB.Text != _currentClient[2])
            {
                ShowAndHideError("Данная электронная почта\nуже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                Close();
                return;
            }

            string fullName = fullNameTB.Text.Trim();
            string[] parts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
            {
                ShowAndHideError("Неверно указано ФИО!");
                return;
            }

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length > 0)
                {
                    parts[i] = char.ToUpper(parts[i][0]) +
                               (parts[i].Length > 1 ? parts[i].Substring(1).ToLower() : "");
                }
            }

            fullName = string.Join(" ", parts);

            if (emailTB.Visibility == Visibility.Visible)
            {
                double newWallet = accountWallet + double.Parse(_currentClient[5], CultureInfo.InvariantCulture);
                if (!Manager.dataBase.completeCommand($"UPDATE `client` SET `phone_number` = '{phoneTB.Text}', `email` = '{emailTB.Text}', " +
                                                             $"`full_name` = '{fullName}', `wallet_account` = '{newWallet.ToString().Replace(',', '.')}' WHERE `id` = {_currentClient[0]}"))
                {
                    Close();
                    return;
                }
            }
            else
            {
                double newWallet = accountWallet + double.Parse(_currentClient[5], CultureInfo.InvariantCulture);
                if (!Manager.dataBase.completeCommand($"UPDATE `client` SET `phone_number` = '{phoneTB.Text}', " +
                                                             $"`full_name` = '{fullName}', `wallet_account` = '{newWallet.ToString().Replace(',', '.')}' WHERE `id` = {_currentClient[0]}"))
                {
                    Close();
                    return;
                }
            }

            var changes = new List<string>();
            var oldValues = _currentClient;
            var newValues = new List<string> { 
                "",
                phoneTB.Text,
                emailTB.Text,
                "",
                fullName,
                "",
                "",
                "",
                ""
            };

            var fieldNames = new List<string> { 
                "",             
                "Телефон",      
                "Email",        
                "",             
                "ФИО",          
                "",             
                "",             
                "",             
                ""              
            };

            var editableFields = new List<int> { 1, 2, 4 };

            foreach (int i in editableFields)
            {
                if (oldValues[i] != newValues[i])
                {
                    changes.Add($"{fieldNames[i]}: \"{oldValues[i]}\" -> \"{newValues[i]}\"");
                }
            }

            string actionMessage;
            if (changes.Count > 0)
            {
                actionMessage = $"Изменил клиента id {_currentClient[0]} ({_currentClient[4]}). {string.Join(". ", changes)}.";
                Manager.dataBase.addEmployeeAction(
                                                    (uint)Manager.user[0],
                                                    actionMessage,
                                                    DateTime.Now
                                                    );
            }

            if (accountWallet > 0)
            {
                Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Пополнил счёт клиента id {_currentClient[0]} ({fullName}) на {accountWallet.ToString().Replace(',', '.')} руб.", DateTime.Now);
            }
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when phoneTB.IsFocused && emailTB.Visibility == Visibility.Collapsed:
                    fullNameTB.Focus();
                    break;

                case Key.Down when phoneTB.IsFocused && emailTB.Visibility == Visibility.Collapsed:
                    fullNameTB.Focus();
                    break;

                case Key.Return when phoneTB.IsFocused && emailTB.Visibility == Visibility.Visible:
                    emailTB.Focus();
                    break;

                case Key.Down when phoneTB.IsFocused && emailTB.Visibility == Visibility.Visible:
                    emailTB.Focus();
                    break;

                case Key.Return when emailTB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Down when emailTB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Up when emailTB.IsFocused:
                    phoneTB.Focus();
                    break;

                case Key.Return when fullNameTB.IsFocused:
                    walletTB.Focus();
                    break;

                case Key.Down when fullNameTB.IsFocused:
                    walletTB.Focus();
                    break;

                case Key.Up when fullNameTB.IsFocused && emailTB.Visibility == Visibility.Collapsed:
                    phoneTB.Focus();
                    break;

                case Key.Up when fullNameTB.IsFocused && emailTB.Visibility == Visibility.Visible:
                    emailTB.Focus();
                    break;

                case Key.Return when walletTB.IsFocused:
                    saveButton_Click(sender, e);
                    break;

                case Key.Up when walletTB.IsFocused:
                    fullNameTB.Focus();
                    break;
            }
        }
    }
}

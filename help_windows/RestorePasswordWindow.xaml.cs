using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Provider.help_windows
{
    public partial class RestorePasswordWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public string result { get; private set; }

        public RestorePasswordWindow(string mainTheme, string? email = null)
        {
            InitializeComponent();

            checkTheme(mainTheme);

            if (email != null)
                emailTB.Text = email;
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
            showError.Begin(errorLabel);

            try
            {
                await Task.Delay(2500, cancellationToken);
                hideError.Begin(errorLabel);
            }
            catch
            {
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (result == null)
                result = "cancelled";
        }

        private readonly string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
        private string lastEmail;
        private bool isAcceptCodeExist = false;
        private int fourDigitNumber;
        private bool isEmployee = false;

        private void restoreButton_Click(object sender, RoutedEventArgs e)
        {
            isEmployee = false;
            acceptCodeTB.IsEnabled = false;

            if (emailTB.Text == string.Empty || passwordPB.Password == string.Empty || acceptPasswordPB.Password == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить\nвсе поля!");
                return;
            }

            if (!Regex.Match(emailTB.Text, pattern, RegexOptions.IgnoreCase).Success || emailTB.Text.Contains(' '))
            {
                ShowAndHideError("Некорректный e-mail\nадрес!");
                return;
            }

            if (passwordPB.Password.Length <= 5)
            {
                ShowAndHideError("Пароль должен быть\nне меньше 6 символов!");
                return;
            }

            if (passwordPB.Password.Contains(' '))
            {
                ShowAndHideError("В пароле не должно\nбыть пробелов!");
                return;
            }

            if (passwordPB.Password != acceptPasswordPB.Password)
            {
                ShowAndHideError("Подтверждение пароля\nне совпадает!");
                return;
            }

            string isEmailRegistered = Manager.dataBase.isRecordExists("client", "email", emailTB.Text);

            if (isEmailRegistered == "false")
            {
                isEmailRegistered = Manager.dataBase.isRecordExists("employee", "email", emailTB.Text);

                if (isEmailRegistered == "false")
                {
                    ShowAndHideError("Введён неверный\nэлектронный адрес!");
                    isAcceptCodeExist = false;
                    fourDigitNumber = 0;

                    return;
                }
                else if (isEmailRegistered == "error")
                {
                    result = "error";
                    Close();
                    return;
                }
                else
                {
                    isEmployee = true;
                }
            }
            else if (isEmailRegistered == "error")
            {
                result = "error";
                Close();
                return;
            }

            acceptCodeTB.IsEnabled = true;
            acceptCodeTB.Focus();

            if (!isAcceptCodeExist)
            {
                lastEmail = emailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);
                isAcceptCodeExist = true;

                return;
            }

            if (lastEmail != emailTB.Text)
            {
                lastEmail = emailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (acceptCodeTB.Text != fourDigitNumber.ToString())
            {
                ShowAndHideError("Введён неверный\nкод подтверждения!");

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (isEmployee)
            {
                List<object> user = Manager.dataBase.getEmployee("email", emailTB.Text);

                if (!user.Any())
                {
                    result = "error";
                    Close();
                    return;
                }

                if ((uint)user[3] == 2)
                {
                    result = "blocked";
                    Close();
                    return;
                }

                Manager.user = user;

                if (!Manager.dataBase.updatePassword("employee", (uint)Manager.user[0], passwordPB.Password))
                {
                    result = "error";
                    Close();
                    return;
                }

                Manager.dataBase.addEmployeeAction((uint)Manager.user[0], "Восстановил пароль", DateTime.Now);

                List<string> toSaveEmployee = ["employee", Manager.user[0].ToString()];

                if (File.Exists("resources/data.sav"))
                {
                    File.Delete("resources/data.sav");
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSaveEmployee));
                }
                else
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSaveEmployee));

                result = "employee";
                Close();
                return;
            }

            Manager.user = Manager.dataBase.getClient("email", emailTB.Text);

            if (!Manager.user.Any())
            {
                result = "error";
                Close();
                return;
            }

            if (!Manager.dataBase.updatePassword("client", (uint)Manager.user[0], passwordPB.Password))
            {
                result = "error";
                Close();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], "Восстановил пароль", DateTime.Now);

            List<string> toSaveClient = ["client", Manager.user[0].ToString()];

            if (File.Exists("resources/data.sav"))
            {
                File.Delete("resources/data.sav");
                File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSaveClient));
            }
            else
                File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSaveClient));

            result = "client";
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when emailTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Down when emailTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Return when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Down when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Up when passwordPB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when acceptPasswordPB.IsFocused && !acceptCodeTB.IsEnabled:
                    restoreButton_Click(sender, e);
                    break;

                case Key.Return when acceptPasswordPB.IsFocused && acceptCodeTB.IsEnabled:
                    acceptCodeTB.Focus();
                    break;

                case Key.Down when acceptPasswordPB.IsFocused && acceptCodeTB.IsEnabled:
                    acceptCodeTB.Focus();
                    break;

                case Key.Up when acceptPasswordPB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Up when acceptCodeTB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Return when acceptCodeTB.IsFocused:
                    restoreButton_Click(sender, e);
                    break;
            }
        }
    }
}

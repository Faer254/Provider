using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MaterialDesignThemes.Wpf;
using Provider.classes;
using Provider.help_windows;

namespace Provider.windows
{
    public partial class AuthorizationWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public AuthorizationWindow()
        {
            InitializeComponent();

            checkTheme();
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var screenPosition = this.PointToScreen(mousePosition);
            double relativeX = mousePosition.X / ActualWidth;

            if (mousePosition.Y <= 26)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;

                    var point = PointToScreen(mousePosition);


                    Left = screenPosition.X - (relativeX * ActualWidth);
                    Top = screenPosition.Y - 13;

                    DragMove();
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void checkTheme()
        {
            ITheme theme = paletteHelper.GetTheme();

            if (Manager.theme == "light")
            {
                theme.SetBaseTheme(Theme.Light);
                themeToggle.IsChecked = false;
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                themeToggle.IsChecked = true;
            }

            paletteHelper.SetTheme(theme);
        }

        private void themeToggle_Click(object sender, RoutedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (theme.GetBaseTheme() == BaseTheme.Dark)
            {
                theme.SetBaseTheme(Theme.Light);
                Manager.theme = "light";
                File.WriteAllText("resources/theme.dat", "light");
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                Manager.theme = "dark";
                File.WriteAllText("resources/theme.dat", "dark");
            }

            paletteHelper.SetTheme(theme);
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

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }


        private int animationDuration = 200;
        private async void alternateEntryButton1_Click(object sender, RoutedEventArgs e)
        {
            var showElement = (Storyboard)FindResource("ShowElement");
            var hideElement = (Storyboard)FindResource("HideElement");

            hideElement.Begin(logInPanel);

            await Task.Delay(animationDuration);

            logInPanel.Visibility = Visibility.Collapsed;

            phoneLogInPanel.Visibility = Visibility.Visible;
            showElement.Begin(phoneLogInPanel);

            await Task.Delay(animationDuration);
        }

        private async void alternateEntryButton_Click(object sender, RoutedEventArgs e)
        {
            var showElement = (Storyboard)FindResource("ShowElement");
            var hideElement = (Storyboard)FindResource("HideElement");

            hideElement.Begin(phoneLogInPanel);

            await Task.Delay(animationDuration);

            phoneLogInPanel.Visibility = Visibility.Collapsed;

            logInPanel.Visibility = Visibility.Visible;
            showElement.Begin(logInPanel);

            await Task.Delay(animationDuration);
        }

        private string lastPhone;
        private bool isAcceptCodeExist = false;
        private int fourDigitNumber;

        private void phoneLogInButton_Click(object sender, RoutedEventArgs e)
        {
            acceptCodeTB.IsEnabled = false;

            if (phoneTB.Text.Contains('_'))
            {
                ShowAndHideError("Необходимо ввести номер телефона!");
                isAcceptCodeExist = false;
                fourDigitNumber = 0;

                return;
            }

            string isPhoneRegistered = Manager.dataBase.isRecordExists("client", "phone_number", phoneTB.Text);

            if (isPhoneRegistered == "false" ||
                isPhoneRegistered == "error")
            {
                ShowAndHideError("Введён неверный номер телефона!");
                isAcceptCodeExist = false;
                fourDigitNumber = 0;

                return;
            }

            acceptCodeTB.IsEnabled = true;
            acceptCodeTB.Focus();

            if (!isAcceptCodeExist)
            {
                lastPhone = phoneTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код авторизации", MessageBoxButton.OK, MessageBoxImage.Information);
                isAcceptCodeExist = true;

                return;
            }

            if (lastPhone != phoneTB.Text)
            {
                lastPhone = phoneTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код авторизации", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }
            
            if (acceptCodeTB.Text != fourDigitNumber.ToString())
            {
                ShowAndHideError("Введён неверный код подтверждения!");

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код авторизации", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            Manager.user = Manager.dataBase.getClient("phone_number", phoneTB.Text);

            List<string> toSave = ["client", Manager.user[0].ToString()];
            
            if (File.Exists("resources/data.sav"))
            {
                File.Delete("resources/data.sav");
                File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));
            }
            else
                File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));

            new ClientWindow().Show();
            Close();
        }
        
        private void logInButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTB.Text == string.Empty || passwordPB.Password == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить все поля!");
                return;
            }

            List<object> user = Manager.dataBase.getUserByLoginAndPassword("client", loginTB.Text, passwordPB.Password);

            if (user.Any())
            {
                Manager.user = user;

                List<string> toSave = ["client", Manager.user[0].ToString()];

                if (File.Exists("resources/data.sav"))
                {
                    File.Delete("resources/data.sav");
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));
                }
                else
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));

                new ClientWindow().Show();
                Close();
            }

            user = Manager.dataBase.getUserByLoginAndPassword("employee", loginTB.Text, passwordPB.Password);

            if (user.Any())
            {
                if ((uint)user[3] == 2)
                {
                    ShowAndHideError("Ваш аккаунт заблокирован!");
                    return;
                }

                Manager.user = user;

                List<string> toSave = ["employee", Manager.user[0].ToString()];

                if (File.Exists("resources/data.sav"))
                {
                    File.Delete("resources/data.sav");
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));
                }
                else
                    File.WriteAllBytes("resources/data.sav", Encrypter.EncryptListToBytes_AES(toSave));

                new EmployeeWindow().Show();
                Close();
            }

            ShowAndHideError("Неверный логин или пароль!");
        }

        private void restorePassButton_Click(object sender, RoutedEventArgs e)
        {
            var restorePassResult = new RestorePasswordWindow(Manager.theme, loginTB.Text == string.Empty ? null : loginTB.Text);
            restorePassResult.ShowDialog();
            switch (restorePassResult.result)
            {
                case "client":
                    new ClientWindow().Show();
                    Close();
                    break;

                case "employee":
                    new EmployeeWindow().Show();
                    Close();
                    break;

                case "blocked":
                    ShowAndHideError("Ваш аккаунт заблокирован!");
                    break;

                case "error":
                    ShowAndHideError("Ошибка!");
                    break;
            }
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when phoneTB.IsFocused || acceptCodeTB.IsFocused:
                    phoneLogInButton_Click(sender, e);
                    break;

                case Key.Up when acceptCodeTB.IsFocused:
                    phoneTB.Focus();
                    break;

                case Key.Return when loginTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Down when loginTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Return when passwordPB.IsFocused:
                    logInButton_Click(sender, e);
                    break;

                case Key.Up when passwordPB.IsFocused:
                    loginTB.Focus();
                    break;
            }
        }

        private void aboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutUsWindow().ShowDialog();
        }

        private void aboutProgramButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutProgramWindow().ShowDialog();
        }
    }
}
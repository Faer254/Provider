using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Provider.windows_employee
{
    public partial class AddEmployee : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public AddEmployee()
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
            if (loginTB.Text == string.Empty || emailTB.Text == string.Empty || phoneTB.Text.Contains('_') || passwordPB.Password == string.Empty || fullNameTB.Text == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить\nвсе поля!");
                return;
            }

            if (loginTB.Text.Contains(' '))
            {
                ShowAndHideError("В логине не должно\nбыть пробелов!");
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

            string isLoginRegistered = Manager.dataBase.isRecordExists("employee", "login", loginTB.Text);

            if (isLoginRegistered == "true")
            {
                ShowAndHideError("Данный логин\nуже занят!");
                return;
            }

            if (isLoginRegistered == "error")
            {
                Close();
                return;
            }

            if (!Regex.Match(emailTB.Text, pattern, RegexOptions.IgnoreCase).Success || emailTB.Text.Contains(' '))
            {
                ShowAndHideError("Некорректный e-mail\nадрес!");
                return;
            }

            string isEmailRegistered = Manager.dataBase.isRecordExists("employee", "email", emailTB.Text);

            if (isEmailRegistered == "true")
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

            if (isEmailRegistered == "true")
            {
                ShowAndHideError("Данная электронная почта\nуже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                Close();
                return;
            }

            string isPhoneRegistered = Manager.dataBase.isRecordExists("employee", "phone_number", phoneTB.Text);

            if (isPhoneRegistered == "true")
            {
                ShowAndHideError("Данный номер телефона\nуже занят!");
                return;
            }

            if (isPhoneRegistered == "error")
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

            int roleID = roleCB.Text == "Администратор" ? 1 : 2;
            int statusID = statusCB.Text == "Нет блокировки" ? 1 : 2;

            if (!Manager.dataBase.addEmployee(
                                            login: loginTB.Text,
                                            email: emailTB.Text,
                                            password: passwordPB.Password,
                                            statusId: statusID,
                                            fullName: fullName,
                                            roleId: roleID,
                                            phoneNumber: phoneTB.Text))
            {
                Close();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Добавил сотрудника ({fullName})", DateTime.Now);
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when loginTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Down when loginTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when emailTB.IsFocused:
                    phoneTB.Focus();
                    break;

                case Key.Down when emailTB.IsFocused:
                    phoneTB.Focus();
                    break;

                case Key.Up when emailTB.IsFocused:
                    loginTB.Focus();
                    break;

                case Key.Return when phoneTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Down when phoneTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Up when phoneTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when passwordPB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Down when passwordPB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Up when passwordPB.IsFocused:
                    phoneTB.Focus();
                    break;
                
                case Key.Return when fullNameTB.IsFocused:
                    roleCB.Focus();
                    break;

                case Key.Down when fullNameTB.IsFocused:
                    roleCB.Focus();
                    break;

                case Key.Up when fullNameTB.IsFocused:
                    passwordPB.Focus();
                    break;
            }
        }
    }
}

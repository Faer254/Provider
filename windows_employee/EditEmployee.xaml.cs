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
    public partial class EditEmployee : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private List<string> _currentEmployee;
        public EditEmployee(List<string> employee)
        {
            InitializeComponent();
            _currentEmployee = employee;
            checkTheme(Manager.theme);

            loginTB.Text = _currentEmployee[1];
            emailTB.Text = _currentEmployee[2];
            fullNameTB.Text = _currentEmployee[4];
            phoneTB.Text = _currentEmployee[6];

            if (Convert.ToInt32(_currentEmployee[3]) == 1)
            {
                statusCB.Text = "Нет блокировки";
                _currentEmployee[3] = "Нет блокировки";
            }
            else
            {
                statusCB.Text = "Заблокирован";
                _currentEmployee[3] = "Заблокирован";
            }

            if (Convert.ToInt32(_currentEmployee[5])  == 1)
            {
                roleCB.Text = "Администратор";
                _currentEmployee[5] = "Администратор";
            }
            else
            {
                roleCB.Text = "Менеджер";
                _currentEmployee[5] = "Менеджер";
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
            if (loginTB.Text == _currentEmployee[1] && emailTB.Text == _currentEmployee[2] && fullNameTB.Text == _currentEmployee[4] && phoneTB.Text == _currentEmployee[6] &&
                statusCB.Text == _currentEmployee[3] && roleCB.Text == _currentEmployee[5])
            {
                Close();
                return;
            }
            
            if (loginTB.Text == string.Empty || emailTB.Text == string.Empty || phoneTB.Text.Contains('_') || fullNameTB.Text == string.Empty)
            {
                ShowAndHideError("Необходимо заполнить\nвсе поля!");
                return;
            }

            if (loginTB.Text.Contains(' '))
            {
                ShowAndHideError("В логине не должно\nбыть пробелов!");
                return;
            }

            string isLoginRegistered = Manager.dataBase.isRecordExists("employee", "login", loginTB.Text);

            if (isLoginRegistered == "true" && loginTB.Text != _currentEmployee[1])
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

            if (isEmailRegistered == "true" && emailTB.Text != _currentEmployee[2])
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

            if (isEmailRegistered == "true" && emailTB.Text != _currentEmployee[2])
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

            if (isPhoneRegistered == "true" && phoneTB.Text != _currentEmployee[6])
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

            if (!Manager.dataBase.completeCommand($"UPDATE `employee` SET `login` = '{loginTB.Text}', `email` = '{emailTB.Text}', `status_id` = {statusID}, " +
                                                         $"`full_name` = '{fullName}', `role_id` = {roleID}, `phone_number` = '{phoneTB.Text}' WHERE `id` = {_currentEmployee[0]}"))
            {
                Close();
                return;
            }

            var changes = new List<string>();
            var oldValues = _currentEmployee;
            var newValues = new[] { "", loginTB.Text, emailTB.Text, statusCB.Text, fullName, roleCB.Text, phoneTB.Text, _currentEmployee[7] };

            var fieldNames = new[] { "", "Логин", "Email", "Статус", "ФИО", "Роль", "Телефон", "" };

            for (int i = 1; i < oldValues.Count; i++)
            {
                if (oldValues[i] != newValues[i])
                {
                    changes.Add($"{fieldNames[i]}: \"{oldValues[i]}\" -> \"{newValues[i]}\"");
                }
            }

            string actionMessage = $"Изменил сотрудника id {_currentEmployee[0]} ({_currentEmployee[4]}). {string.Join(". ", changes)}.";

            Manager.dataBase.addEmployeeAction(
                (uint)Manager.user[0],
                actionMessage,
                DateTime.Now
            );
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
                    fullNameTB.Focus();
                    break;

                case Key.Down when phoneTB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Up when phoneTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when fullNameTB.IsFocused:
                    roleCB.Focus();
                    break;

                case Key.Down when fullNameTB.IsFocused:
                    roleCB.Focus();
                    break;

                case Key.Up when fullNameTB.IsFocused:
                    phoneTB.Focus();
                    break;
            }
        }
    }
}

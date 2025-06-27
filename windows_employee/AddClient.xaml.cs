using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Provider.windows_employee
{
    public partial class AddClient : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public AddClient()
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

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (phoneTB.Text.Contains('_'))
            {
                ShowAndHideError("Необходимо указать\nномер телефона!");
                return;
            }

            string isPhoneRegistered = Manager.dataBase.isRecordExists("client", "phone_number", phoneTB.Text);

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

            if (!Manager.dataBase.addClient(
                                            phoneNumber: phoneTB.Text,
                                            fullName: fullName))
            {
                Close();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Добавил клиента ({fullName})", DateTime.Now);
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when phoneTB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Down when phoneTB.IsFocused:
                    fullNameTB.Focus();
                    break;

                case Key.Return when fullNameTB.IsFocused:
                    saveButton_Click(sender, e);
                    break;

                case Key.Up when fullNameTB.IsFocused:
                    phoneTB.Focus();
                    break;
            }
        }
    }
}

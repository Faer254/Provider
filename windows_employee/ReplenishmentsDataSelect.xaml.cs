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
    public partial class ReplenishmentsDataSelect : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public ReplenishmentsDataSelect()
        {
            InitializeComponent();
            checkTheme(Manager.theme);

            firstDate.Text = DateTime.Today.AddMonths(-1).ToString("dd.MM.yyyy");
            secondDate.Text = DateTime.Today.ToString("dd.MM.yyyy");
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

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (firstDate.Text == string.Empty || secondDate.Text == string.Empty)
            {
                ShowAndHideError("Необходимо указать\nобе даты!");
                return;
            }

            DateTime first = Convert.ToDateTime(firstDate.Text);
            DateTime second = Convert.ToDateTime(secondDate.Text);

            if (first > second)
            {
                ShowAndHideError("Неверно указан\nдиапазон!");
                return;
            }

            first = new DateTime(first.Year, first.Month, first.Day, 0, 0, 0);
            second = new DateTime(second.Year, second.Month, second.Day, 23, 59, 59);

            Close();
            Manager.reports.GenerateWalletReplenishmentReport(first, second);
            return;
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when firstDate.IsFocused:
                    secondDate.Focus();
                    break;

                case Key.Down when firstDate.IsFocused:
                    secondDate.Focus();
                    break;

                case Key.Up when secondDate.IsFocused:
                    firstDate.Focus();
                    break;

                case Key.Return when secondDate.IsFocused:
                    saveButton_Click(sender, e);
                    break;
            }
        }
    }
}

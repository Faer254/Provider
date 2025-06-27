using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Provider.help_windows
{
    public partial class ErrorWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public ErrorWindow(string error, string mainTheme)
        {
            InitializeComponent();

            checkTheme(mainTheme);

            errorTextBlock.Text = error;

            okButton.Focus();
        }

        private void checkTheme(string mainTheme)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (mainTheme == "light")
            {
                theme.SetBaseTheme(Theme.Light);

                closeImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/close-icon.png"));
                errorImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/error-icon.png"));
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);

                mainBorder.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494949"));
                windowChromeBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#494949"));

                closeImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/close-icon-white.png"));
                errorImage.Source = new BitmapImage(new Uri("pack://application:,,,/resources/images/error-icon-white.png"));
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
    }
}

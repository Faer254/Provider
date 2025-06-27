using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Provider.help_windows
{
    public partial class AboutUsWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public AboutUsWindow()
        {
            InitializeComponent();

            checkTheme(Manager.theme);
        }

        private void checkTheme(string mainTheme)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (mainTheme != "light")
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

        private void exit(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

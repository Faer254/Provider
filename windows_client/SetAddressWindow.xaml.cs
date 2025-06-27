using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace Provider.windows_client
{
    public partial class SetAddressWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        public string Address { get; private set; }

        public SetAddressWindow()
        {
            InitializeComponent();
            Address = string.Empty;

            checkTheme(Manager.theme);
            addressTB.Focus();
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

            if (mousePosition.Y <= 26)
            {
                DragMove();
            }
        }

        private void exit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (addressTB.Text == string.Empty)
                return;

            Address = addressTB.Text;
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                addButton_Click(sender, e);
        }
    }
}

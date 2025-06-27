using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Provider.windows_employee
{
    public partial class AcceptCodeWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private int fourDigitNumber;

        public AcceptCodeWindow()
        {
            InitializeComponent();
            checkTheme(Manager.theme);
            codeTB.Focus();

            Random random = new Random();
            fourDigitNumber = random.Next(1000, 10000);
            MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);
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
            DialogResult = false;
            Close();
        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (codeTB.Text.Contains('_'))
            {
                DialogResult = false;
                Close();
                return;
            }

            if (codeTB.Text != fourDigitNumber.ToString())
            {
                DialogResult = false;
                Close();
                return;
            }

            DialogResult = true;
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                acceptButton_Click(sender, e);
        }
    }
}

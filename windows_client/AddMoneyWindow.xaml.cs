using MaterialDesignThemes.Wpf;
using Provider.classes;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Provider.windows_client
{
    public partial class AddMoneyWindow : Window
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public AddMoneyWindow()
        {
            InitializeComponent();
            checkTheme(Manager.theme);
            moneyInputTB.Focus();
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
            double newAccountWallet = (float)Manager.user[5] + double.Parse(moneyInputTB.Text, CultureInfo.InvariantCulture);
            Manager.dataBase.completeCommand($"UPDATE `client` SET `wallet_account`='{newAccountWallet.ToString("0.00").Replace(',', '.')}' WHERE `id`={Manager.user[0]}");
            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Пополнил счёт на {double.Parse(moneyInputTB.Text, CultureInfo.InvariantCulture).ToString().Replace(',', '.')} руб.", DateTime.Now);
            Close();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) 
                addButton_Click(sender, e);
        }
    }
}

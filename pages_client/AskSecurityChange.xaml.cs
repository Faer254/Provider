using Provider.windows;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_client
{
    public partial class AskSecurityChange : Page
    {
        private readonly ClientWindow clientWindow;
        private readonly Frame secureFrame;

        public AskSecurityChange(ClientWindow window, Frame frame)
        {
            InitializeComponent();
            clientWindow = window;
            secureFrame = frame;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            secureFrame.Navigate(new ChangeEmail(clientWindow));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            secureFrame.Navigate(new ChangePassword(clientWindow));
        }
    }
}

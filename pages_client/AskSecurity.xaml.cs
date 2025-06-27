using Provider.windows;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_client
{
    public partial class AskSecurity : Page
    {
        private readonly ClientWindow clientWindow;
        private readonly Frame secureFrame;
        
        public AskSecurity(ClientWindow window, Frame frame)
        {
            InitializeComponent();
            clientWindow = window;
            secureFrame = frame;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            secureFrame.Navigate(new AddSecurity(clientWindow));
        }
    }
}

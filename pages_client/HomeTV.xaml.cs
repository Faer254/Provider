using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using Provider.windows_client;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_client
{
    public partial class HomeTV : Page
    {
        private readonly ClientWindow clientWindow;

        public HomeTV(ClientWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            clientWindow = window;
            loadServices();
        }

        private DataView dataView;
        private DataTable dataTable;
        private List<object> clientService;

        private void loadServices()
        {
            serviceNameLabel.Visibility = Visibility.Collapsed;
            deleteServiceButton.Visibility = Visibility.Collapsed;
            descriptionServiceBorder.Visibility = Visibility.Collapsed;
            payStackPanel.Visibility = Visibility.Collapsed;

            if (dataTable != null)
            {
                dataTable.Clear();
                dataView = null;
                dataGrid.ItemsSource = null;
            }

            clientService = Manager.dataBase.getClientHomeService((uint)Manager.user[0], 3);

            if (clientService == null)
            {
                clientWindow.loadCLient();
                return;
            }

            if (clientService.Any())
                dataTable = Manager.dataBase.getHomeServices(3, clientService[1].ToString());
            else
                dataTable = Manager.dataBase.getHomeServices(3);

            if (clientService.Any())
            {
                serviceNameLabel.Visibility = Visibility.Visible;
                deleteServiceButton.Visibility = Visibility.Visible;
                descriptionServiceBorder.Visibility = Visibility.Visible;
                payStackPanel.Visibility = Visibility.Visible;

                serviceNameLabel.Content = clientService[4];

                descriptionLabel.Content = clientService[5];

                costLabel.Content = $"{clientService[6]} руб.,";
                dateToPayLabel.Content = ((DateTime)clientService[2]).ToString("d MMMM");
            }

            dataView = new DataView(dataTable);
            dataView.Sort = "id DESC";
            dataGrid.ItemsSource = dataView;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            clientWindow.hideWorkFrame();
        }

        private void finderTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = finderTB.Text;

            try
            {
                dataView.RowFilter = "";

                if (!string.IsNullOrEmpty(searchText))
                    dataView.RowFilter = $"service_name LIKE '%{searchText}%' OR Convert(cost, 'System.String') LIKE '%{searchText}%' OR description LIKE '%{searchText}%'";
                else
                    dataView.RowFilter = "";
            }
            catch
            {
                dataView.RowFilter = "";
            }
        }

        private void deleteServiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new QuestionWindow("Вы уверены, что хотите отключить текущую услугу?", Manager.theme).ShowDialog())
            {
                return;
            }

            if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `client_id`={Manager.user[0]} AND `id`={clientService[0]}"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Отключил услугу id {clientService[1]} ({clientService[4]})", DateTime.Now);

            loadServices();
            clientWindow.loadCLient();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Manager.user[9])
            {
                clientWindow.ShowAndHideError("Вам необходимо сначала погасить задолженность!");
                return;
            }

            if (!(bool)new QuestionWindow("Вы уверены, что хотите подключить новую услугу?", Manager.theme).ShowDialog())
            {
                return;
            }

            Button button = sender as Button;
            DataRowView rowView = button.DataContext as DataRowView;
            int rowIndex = dataGrid.Items.IndexOf(rowView);
            DataRow row = dataView[rowIndex].Row;

            List<string> dataFromRow = new List<string>();
            foreach (DataColumn column in dataTable.Columns)
            {
                dataFromRow.Add(row[column.ColumnName].ToString());
            }

            double newAccountWallet = (float)Manager.user[5] - Convert.ToDouble(dataFromRow[4]);

            if (newAccountWallet < 0)
            {
                clientWindow.ShowAndHideError("Недостаточно средств на счету!");
                return;
            }

            string address;

            if (!clientService.Any())
            {
                new InformationWindow("Необходимо указать адрес", Manager.theme).ShowDialog();
                SetAddressWindow window = new SetAddressWindow();
                window.ShowDialog();
                address = window.Address;

                if (address == string.Empty)
                    return;
            }
            else
                address = (string)clientService[3];

            if (clientService.Any())
            {
                if ((bool)new QuestionWindow("Вы хотите сменить адрес подключения услуги?", Manager.theme).ShowDialog())
                {
                    SetAddressWindow window1 = new SetAddressWindow();
                    window1.ShowDialog();

                    if (window1.Address == string.Empty)
                        return;

                    address = window1.Address;
                }
            }

            if (clientService.Any())
            {
                if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `client_id`={Manager.user[0]} AND `id`={clientService[0]}"))
                {
                    clientWindow.loadCLient();
                    return;
                }

                if ((DateTime)clientService[2] <= DateTime.Now)
                {
                    Manager.dataBase.completeCommand($"UPDATE `client` SET `status_id`=1 WHERE `id`={Manager.user[0]}");
                }

                Manager.dataBase.addClientAction((uint)Manager.user[0], $"Отключил услугу id {clientService[1]} ({clientService[4]})", DateTime.Now);
            }

            if (!Manager.dataBase.completeCommand($"INSERT INTO `concluded_services`(`client_id`, `service_id`, `start_date`, `next_payment`, `address`) VALUES " +
                                                 $"({Manager.user[0]}, {dataFromRow[0]}, '{DateTime.Today.ToString("yyyy.MM.dd")}', '{DateTime.Today.AddMonths(1).ToString("yyyy.MM.dd")}', '{address}')"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.completeCommand($"UPDATE `client` SET `wallet_account`='{newAccountWallet.ToString("0.00").Replace(',', '.')}' WHERE `id`={Manager.user[0]}");
            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Подключил услугу id {dataFromRow[0]} ({dataFromRow[1]})", DateTime.Now);

            clientWindow.loadCLient();
            loadServices();
        }
    }
}

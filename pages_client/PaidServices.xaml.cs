using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using Provider.windows_client;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_client
{
    public partial class PaidServices : Page
    {
        private readonly ClientWindow clientWindow;

        public PaidServices(ClientWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            clientWindow = window;
            loadConnected();
            loadUnConnected();
        }

        private DataView dataViewConnected;
        private DataTable dataTableConnected;

        private void loadConnected()
        {

            if (dataTableConnected != null)
            {
                dataTableConnected.Clear();
                dataViewConnected = null;
                dataGridConnected.ItemsSource = null;
            }

            dataTableConnected = Manager.dataBase.getPaidConnectedServices((uint)Manager.user[0]);

            dataViewConnected = new DataView(dataTableConnected);
            dataGridConnected.ItemsSource = dataViewConnected;
        }

        private DataView dataViewUnConnected;
        private DataTable dataTableUnConnected;

        private void loadUnConnected()
        {

            if (dataTableUnConnected != null)
            {
                dataTableUnConnected.Clear();
                dataViewUnConnected = null;
                dataGridUnConnected.ItemsSource = null;
            }

            dataTableUnConnected = Manager.dataBase.getPaidUnConnectedServices((uint)Manager.user[0]);

            dataViewUnConnected = new DataView(dataTableUnConnected);
            dataGridUnConnected.ItemsSource = dataViewUnConnected;
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            clientWindow.hideWorkFrame();
        }

        private void finderTBConnected_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = finderTBConnected.Text;

            try
            {
                dataViewConnected.RowFilter = "";

                if (!string.IsNullOrEmpty(searchText))
                    dataViewConnected.RowFilter = $"service_name LIKE '%{searchText}%' OR Convert(cost, 'System.String') LIKE '%{searchText}%' OR Convert(next_payment, 'System.String') LIKE '%{searchText}%' OR description LIKE '%{searchText}%'";
                else
                    dataViewConnected.RowFilter = "";
            }
            catch
            {
                dataViewConnected.RowFilter = "";
            }
        }

        private void disConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new QuestionWindow("Вы уверены, что хотите отключить данную услугу?", Manager.theme).ShowDialog())
            {
                return;
            }

            Button button = sender as Button;
            DataRowView rowView = button.DataContext as DataRowView;
            int rowIndex = dataGridConnected.Items.IndexOf(rowView);
            DataRow row = dataViewConnected[rowIndex].Row;

            List<string> dataFromRow = new List<string>();
            foreach (DataColumn column in dataTableConnected.Columns)
            {
                dataFromRow.Add(row[column.ColumnName].ToString());
            }

            if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `id`={dataFromRow[0]}"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Отключил услугу id {dataFromRow[1]} ({dataFromRow[3]})", DateTime.Now);
            loadConnected();
            loadUnConnected();
            clientWindow.loadCLient();
        }

        private void finderTBUnConnected_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = finderTBUnConnected.Text;

            try
            {
                dataViewUnConnected.RowFilter = "";

                if (!string.IsNullOrEmpty(searchText))
                    dataViewUnConnected.RowFilter = $"service_name LIKE '%{searchText}%' OR Convert(cost, 'System.String') LIKE '%{searchText}%' OR description LIKE '%{searchText}%'";
                else
                    dataViewUnConnected.RowFilter = "";
            }
            catch
            {
                dataViewUnConnected.RowFilter = "";
            }
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Manager.user[9])
            {
                clientWindow.ShowAndHideError("Вам необходимо сначала погасить задолженность!");
                return;
            }

            if (!(bool)new QuestionWindow("Вы уверены, что хотите подключить данную услугу?", Manager.theme).ShowDialog())
            {
                return;
            }

            Button button = sender as Button;
            DataRowView rowView = button.DataContext as DataRowView;
            int rowIndex = dataGridUnConnected.Items.IndexOf(rowView);
            DataRow row = dataViewUnConnected[rowIndex].Row;

            List<string> dataFromRow = new List<string>();
            foreach (DataColumn column in dataTableUnConnected.Columns)
            {
                dataFromRow.Add(row[column.ColumnName].ToString());
            }

            double newAccountWallet = (float)Manager.user[5] - Convert.ToDouble(dataFromRow[2]);

            if (newAccountWallet < 0)
            {
                clientWindow.ShowAndHideError("Недостаточно средств на счету!");
                return;
            }

            if (dataFromRow[4] == "True")
            {
                new InformationWindow("Необходимо указать адрес", Manager.theme).ShowDialog();
                SetAddressWindow window = new SetAddressWindow();
                window.ShowDialog();
                string address = window.Address;

                if (address == string.Empty)
                    return;

                if (!Manager.dataBase.completeCommand($"INSERT INTO `concluded_services`(`client_id`, `service_id`, `start_date`, `next_payment`, `address`) VALUES " +
                                                      $"({Manager.user[0]}, {dataFromRow[0]}, '{DateTime.Today.ToString("yyyy.MM.dd")}', '{DateTime.Today.AddMonths(1).ToString("yyyy.MM.dd")}', '{address}')"))
                {
                    clientWindow.loadCLient();
                    return;
                }

                Manager.dataBase.completeCommand($"UPDATE `client` SET `wallet_account`='{newAccountWallet.ToString("0.00").Replace(',', '.')}' WHERE `id`={Manager.user[0]}");
                Manager.dataBase.addClientAction((uint)Manager.user[0], $"Подключил услугу id {dataFromRow[0]} ({dataFromRow[1]})", DateTime.Now);
                loadConnected();
                loadUnConnected();
                clientWindow.loadCLient();

                return;
            }

            if (!Manager.dataBase.completeCommand($"INSERT INTO `concluded_services`(`client_id`, `service_id`, `start_date`, `next_payment`) VALUES " +
                                                      $"({Manager.user[0]}, {dataFromRow[0]}, '{DateTime.Today.ToString("yyyy.MM.dd")}', '{DateTime.Today.AddMonths(1).ToString("yyyy.MM.dd")}')"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.completeCommand($"UPDATE `client` SET `wallet_account`='{newAccountWallet.ToString("0.00").Replace(',', '.')}' WHERE `id`={Manager.user[0]}");
            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Подключил услугу id {dataFromRow[0]} ({dataFromRow[1]})", DateTime.Now);
            loadConnected();
            loadUnConnected();
            clientWindow.loadCLient();
        }
    }
}

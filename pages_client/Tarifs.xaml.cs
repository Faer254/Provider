using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_client
{
    public partial class Tarifs : Page
    {
        private readonly ClientWindow clientWindow;

        public Tarifs(ClientWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            clientWindow = window;

            loadTarifs();
        }

        private DataView dataView;
        private DataTable dataTable;
        private List<object> clientTarif;
        private void loadTarifs()
        {
            tarifNameLabel.Visibility = Visibility.Collapsed;
            deleteTarifButton.Visibility = Visibility.Collapsed;
            descriptionTarifBorder.Visibility = Visibility.Collapsed;
            payStackPanel.Visibility = Visibility.Collapsed;

            if (dataTable != null)
            {
                dataTable.Clear();
                dataView = null;
                dataGrid.ItemsSource = null;
            }

            clientTarif = Manager.dataBase.getClientTarif((uint)Manager.user[0]);

            if (clientTarif == null)
            {
                clientWindow.loadCLient();
                return;
            }

            if (clientTarif.Any())
                dataTable = Manager.dataBase.getTarifs(clientTarif[7].ToString());
            else
                dataTable = Manager.dataBase.getTarifs();

            if (clientTarif.Any())
            {
                tarifNameLabel.Visibility = Visibility.Visible;
                deleteTarifButton.Visibility = Visibility.Visible;
                descriptionTarifBorder.Visibility = Visibility.Visible;
                payStackPanel.Visibility = Visibility.Visible;

                tarifNameLabel.Content = clientTarif[2];

                if ((int)clientTarif[4] < 0)
                {
                    internetLabel.Content = "Безлим. Гб";
                }
                else
                {
                    internetLabel.Content = $"{clientTarif[4]} Гб";
                }

                if ((int)clientTarif[5] < 0)
                {
                    phoneLabel.Content = "Безлим. Мин";
                }
                else
                {
                    phoneLabel.Content = $"{clientTarif[5]} Мин";
                }

                if ((int)clientTarif[6] < 0)
                {
                    smsLabel.Content = "Безлим. SMS";
                }
                else
                {
                    smsLabel.Content = $"{clientTarif[6]} Мин";
                }

                costLabel.Content = $"{clientTarif[3]} руб.,";
                dateToPayLabel.Content = ((DateTime)clientTarif[1]).ToString("d MMMM");
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

        private void deleteTarifButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new QuestionWindow("Вы уверены, что хотите отключить текущий тариф?", Manager.theme).ShowDialog())
            {
                return;
            }

            if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `client_id`={Manager.user[0]} AND `id`={clientTarif[0]}"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.completeCommand($"UPDATE `client` SET `internet`=0,`phone`=0,`sms`=0 WHERE `id`={Manager.user[0]}");

            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Отключил услугу id {clientTarif[7]} ({clientTarif[2]})", DateTime.Now);

            loadTarifs();
            clientWindow.loadCLient();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)Manager.user[9])
            {
                clientWindow.ShowAndHideError("Вам необходимо сначала погасить задолженность!");
                return;
            }

            if (!(bool)new QuestionWindow("Вы уверены, что хотите подключить новый тариф?", Manager.theme).ShowDialog())
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

            if (clientTarif.Any())
            {
                if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `client_id`={Manager.user[0]} AND `id`={clientTarif[0]}"))
                {
                    clientWindow.loadCLient();
                    return;
                }

                if ((DateTime)clientTarif[1] <= DateTime.Now)
                {
                    Manager.dataBase.completeCommand($"UPDATE `client` SET `status_id`=1 WHERE `id`={Manager.user[0]}");
                }

                Manager.dataBase.addClientAction((uint)Manager.user[0], $"Отключил услугу id {clientTarif[7]} ({clientTarif[2]})", DateTime.Now);
            }

            if(!Manager.dataBase.completeCommand($"INSERT INTO `concluded_services`(`client_id`, `service_id`, `start_date`, `next_payment`) VALUES " +
                                                 $"({Manager.user[0]}, {dataFromRow[0]}, '{DateTime.Today.ToString("yyyy.MM.dd")}', '{DateTime.Today.AddMonths(1).ToString("yyyy.MM.dd")}')"))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.completeCommand($"UPDATE `client` SET `wallet_account`='{newAccountWallet.ToString("0.00").Replace(',', '.')}', `internet`='{dataFromRow[6]}',`phone`='{dataFromRow[7]}',`sms`='{dataFromRow[8]}' WHERE `id`={Manager.user[0]}");
            Manager.dataBase.addClientAction((uint)Manager.user[0], $"Подключил услугу id {dataFromRow[0]} ({dataFromRow[1]})", DateTime.Now);

            clientWindow.loadCLient();
            loadTarifs();
        }
    }
}

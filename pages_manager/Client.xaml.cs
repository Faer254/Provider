using Microsoft.Win32;
using OfficeOpenXml;
using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using Provider.windows_employee;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_manager
{
    public partial class Client : Page
    {
        private readonly EmployeeWindow _employeeWindow;
        public Client(EmployeeWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            _employeeWindow = window;
            loadTable();

            statusCB.Items.Add(DBNull.Value);
            statusCB.Items.Add("Нет блокировки");
            statusCB.Items.Add("Заблокирован");
            statusCB.SelectedIndex = 0;
        }

        private DataView _dataView;
        private DataTable _dataTable;
        private void loadTable()
        {
            if (_dataTable != null)
            {
                _dataTable.Clear();
                _dataView = null;
                dataGrid.ItemsSource = null;
            }

            _dataTable = Manager.dataBase.getClientTable();
            _dataView = new DataView(_dataTable);
            dataGrid.ItemsSource = _dataView;
            _dataView.RowFilter = rowFilter;
        }

        private void finderTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void roleCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void statusCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private string rowFilter = "";
        private void ApplyCombinedFilter()
        {
            if (_dataView == null) return;

            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(finderTB.Text))
            {
                string searchText = finderTB.Text;
                filters.Add($"(phone_number LIKE '%{searchText}%' OR email LIKE '%{searchText}%' OR full_name LIKE '%{searchText}%')");
            }

            if (statusCB.SelectedIndex > 0)
            {
                int statusId = statusCB.SelectedIndex;
                filters.Add($"status_id = {statusId}");
            }

            rowFilter = filters.Any() ? string.Join(" AND ", filters) : "";
            _dataView.RowFilter = rowFilter;
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dataView == null || _dataView.Count == 0)
            {
                _employeeWindow.ShowAndHideError("Нет данных для экспорта!");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Clients_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Сохранить файл Excel"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Dmitry");
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Лист1");
                        worksheet.Cells.Style.Font.Name = "Calibri";

                        int columnsToExport = dataGrid.Columns.Count - 1;
                        for (int i = 0; i < columnsToExport; i++)
                        {
                            var headerCell = worksheet.Cells[1, i + 1];
                            headerCell.Value = dataGrid.Columns[i].Header;

                            headerCell.Style.Font.Bold = true;
                            headerCell.Style.Font.Size = 14;
                            headerCell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        }

                        for (int i = 0; i < _dataView.Count; i++)
                        {
                            for (int j = 0; j < columnsToExport; j++)
                            {
                                var columnName = dataGrid.Columns[j].SortMemberPath;
                                var cell = worksheet.Cells[i + 2, j + 1];
                                var value = _dataView[i][columnName];

                                cell.Style.Font.Size = 12;

                                cell.Value = value;
                            }
                        }

                        var dataRange = worksheet.Cells[1, 1, _dataView.Count + 1, columnsToExport];

                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    new SuccesWindow("Данные успешно экспортированы!", Manager.theme).ShowDialog();
                }
                catch (Exception ex)
                {
                    new ErrorWindow($"Ошибка при экспорте: {ex.Message}", Manager.theme).ShowDialog();
                }
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            new AddClient().ShowDialog();
            loadTable();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new AcceptCodeWindow().ShowDialog())
            {
                return;
            }

            Button button = sender as Button;
            DataRowView rowView = button.DataContext as DataRowView;

            List<string> dataFromRow = new List<string>();
            foreach (DataColumn column in rowView.DataView.Table.Columns)
            {
                dataFromRow.Add(rowView[column.ColumnName].ToString());
            }

            new EditClient(dataFromRow).ShowDialog();
            loadTable();
        }
    }
}

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
    public partial class ConcludedServices : Page
    {
        private readonly EmployeeWindow _employeeWindow;
        public ConcludedServices(EmployeeWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            _employeeWindow = window;
            loadTable().ConfigureAwait(false);
        }

        private DataView _dataView;
        private DataTable _dataTable;
        private async Task loadTable()
        {
            dataGrid.IsEnabled = false;

            if (_dataTable != null)
            {
                _dataTable.Clear();
                _dataView = null;
                dataGrid.ItemsSource = null;
            }

            await Task.Run(() =>
            {
                _dataTable = Manager.dataBase.getConcludedServices();
                _dataView = new DataView(_dataTable) { Sort = "id DESC" };
            });

            UpdatePagination();
            dataGrid.IsEnabled = true;

            DataView clientView = _dataTable.DefaultView.ToTable(true, "client_id").DefaultView;

            DataTable dtWithEmpty = new DataTable();
            dtWithEmpty.Columns.Add("client_id");

            dtWithEmpty.Rows.Add(new object[] { DBNull.Value });

            foreach (DataRowView row in clientView)
            {
                dtWithEmpty.Rows.Add(row["client_id"]);
            }

            clientCB.ItemsSource = dtWithEmpty.DefaultView;
            clientCB.SelectedIndex = 0;

            DataView typeView = _dataTable.DefaultView.ToTable(true, "service_type_name").DefaultView;

            DataTable dtTypeWithEmpty = new DataTable();
            dtTypeWithEmpty.Columns.Add("service_type_name");

            dtTypeWithEmpty.Rows.Add(new object[] { DBNull.Value });

            foreach (DataRowView row in typeView)
            {
                dtTypeWithEmpty.Rows.Add(row["service_type_name"]);
            }

            typeCB.ItemsSource = dtTypeWithEmpty.DefaultView;
            typeCB.DisplayMemberPath = "service_type_name";
            typeCB.SelectedIndex = 0;
        }

        private int _currentPage = 1;
        private const int PageSize = 30;

        private void UpdatePagination()
        {
            if (_dataView == null) return;

            var filteredData = _dataView.Cast<DataRowView>()
                .Skip((_currentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            dataGrid.ItemsSource = filteredData;
            tbPageInfo.Text = $"Страница {_currentPage} из {GetTotalPages()}";

            btnPrev.IsEnabled = _currentPage > 1;
            btnFirst.IsEnabled = _currentPage > 1;
            btnNext.IsEnabled = _currentPage < GetTotalPages();
            btnLast.IsEnabled = _currentPage < GetTotalPages();
        }

        private int GetTotalPages()
        {
            if (_dataView == null) return 0;
            return (int)Math.Ceiling((double)_dataView.Count / PageSize);
        }

        private void BtnFirst_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            UpdatePagination();
        }

        private void BtnPrev_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdatePagination();
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < GetTotalPages())
            {
                _currentPage++;
                UpdatePagination();
            }
        }

        private void BtnLast_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = GetTotalPages();
            UpdatePagination();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new QuestionWindow("Вы уверены, что хотите отключить данную услугу клиенту?", Manager.theme).ShowDialog())
            {
                return;
            }

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

            if (!Manager.dataBase.completeCommand($"DELETE FROM `concluded_services` WHERE `id` = {dataFromRow[0]}"))
            {
                _employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Отключил услугу id {dataFromRow[2]} ({dataFromRow[7]}) клиенту id {dataFromRow[1]} ({dataFromRow[9]})", DateTime.Now);

            loadTable();
        }

        private void clientCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void typeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void finderTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void ApplyCombinedFilter()
        {
            if (_dataView == null) return;

            var filters = new List<string>();

            if (clientCB.SelectedIndex > 0)
            {
                filters.Add($"client_id = {Convert.ToInt32(clientCB.SelectedValue)}");
            }

            if (typeCB.SelectedIndex > 0)
            {
                filters.Add($"service_type_name = '{typeCB.SelectedValue}'");
            }

            if (!string.IsNullOrWhiteSpace(finderTB.Text))
            {
                string searchText = finderTB.Text;
                filters.Add($"address LIKE '%{searchText}%'");
            }

            _dataView.RowFilter = filters.Any() ? string.Join(" AND ", filters) : "";
            UpdatePagination();
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
                FileName = $"Concluded_services_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
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
    }
}

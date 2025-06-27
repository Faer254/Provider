using Microsoft.Win32;
using OfficeOpenXml;
using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_manager
{
    public partial class Services : Page
    {
        private readonly EmployeeWindow _employeeWindow;
        public Services(EmployeeWindow window)
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
                _dataTable = Manager.dataBase.getServices();
                _dataView = new DataView(_dataTable) { Sort = "id ASC" };
            });

            UpdatePagination();
            dataGrid.IsEnabled = true;

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

            DataView availabilityView = _dataTable.DefaultView.ToTable(true, "availability").DefaultView;

            DataTable dtAvailabilityWithEmpty = new DataTable();
            dtAvailabilityWithEmpty.Columns.Add("availability");

            dtAvailabilityWithEmpty.Rows.Add(new object[] { DBNull.Value });

            foreach (DataRowView row in availabilityView)
            {
                dtAvailabilityWithEmpty.Rows.Add(row["availability"]);
            }

            availabilityCB.ItemsSource = dtAvailabilityWithEmpty.DefaultView;
            availabilityCB.DisplayMemberPath = "availability";
            availabilityCB.SelectedIndex = 0;

            DataView addressView = _dataTable.DefaultView.ToTable(true, "need_an_address").DefaultView;

            DataTable dtAddressWithEmpty = new DataTable();
            dtAddressWithEmpty.Columns.Add("need_an_address");

            dtAddressWithEmpty.Rows.Add(new object[] { DBNull.Value });

            foreach (DataRowView row in addressView)
            {
                dtAddressWithEmpty.Rows.Add(row["need_an_address"]);
            }

            addressCB.ItemsSource = dtAddressWithEmpty.DefaultView;
            addressCB.DisplayMemberPath = "need_an_address";
            addressCB.SelectedIndex = 0;
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

        private void availabilityCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void typeCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyCombinedFilter();
        }

        private void addressCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            if (typeCB.SelectedIndex > 0 && typeCB.SelectedValue != DBNull.Value)
            {
                filters.Add($"service_type_name = '{typeCB.SelectedValue}'");
            }

            if (availabilityCB.SelectedIndex > 0 && availabilityCB.SelectedValue != DBNull.Value)
            {
                filters.Add($"availability = {availabilityCB.SelectedValue}");
            }

            if (addressCB.SelectedIndex > 0 && addressCB.SelectedValue != DBNull.Value)
            {
                filters.Add($"need_an_address = {addressCB.SelectedValue}");
            }

            if (!string.IsNullOrWhiteSpace(finderTB.Text))
            {
                string searchText = finderTB.Text.Replace("'", "''");
                filters.Add($"(service_name LIKE '%{searchText}%' OR description LIKE '%{searchText}%')");
            }

            _dataView.RowFilter = filters.Any() ? string.Join(" AND ", filters) : "";
            _currentPage = 1;
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
                FileName = $"Services_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
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

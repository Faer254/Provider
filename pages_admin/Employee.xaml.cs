﻿using Microsoft.Win32;
using OfficeOpenXml;
using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using Provider.windows_employee;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Provider.pages_admin
{
    public partial class Employee : Page
    {
        private readonly EmployeeWindow _employeeWindow;
        public Employee(EmployeeWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            _employeeWindow = window;
            loadTable();

            roleCB.Items.Add(DBNull.Value);
            roleCB.Items.Add("Администратор");
            roleCB.Items.Add("Менеджер");
            roleCB.SelectedIndex = 0;

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

            _dataTable = Manager.dataBase.getEmployeeTable((uint)Manager.user[0]);
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
                filters.Add($"(login LIKE '%{searchText}%' OR email LIKE '%{searchText}%' OR phone_number LIKE '%{searchText}%' OR full_name LIKE '%{searchText}%')");
            }

            if (roleCB.SelectedIndex > 0)
            {
                int roleId = roleCB.SelectedIndex;
                filters.Add($"role_id = {roleId}");
            }

            if (statusCB.SelectedIndex > 0)
            {
                int statusId = statusCB.SelectedIndex;
                filters.Add($"status_id = {statusId}");
            }

            rowFilter = filters.Any() ? string.Join(" AND ", filters) : "";
            _dataView.RowFilter = rowFilter;
        }

        private DataView _dataViewForExport;
        private DataTable _dataTableForExport;
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
                FileName = $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Сохранить файл Excel"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dataTableForExport = Manager.dataBase.getEmployeeTable(0);
                    _dataViewForExport = new DataView(_dataTableForExport);
                    dataGrid.ItemsSource = _dataViewForExport;
                    _dataViewForExport.RowFilter = rowFilter;

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

                        for (int i = 0; i < _dataViewForExport.Count; i++)
                        {
                            for (int j = 0; j < columnsToExport; j++)
                            {
                                var columnName = dataGrid.Columns[j].SortMemberPath;
                                var cell = worksheet.Cells[i + 2, j + 1];
                                var value = _dataViewForExport[i][columnName];

                                cell.Style.Font.Size = 12;

                                cell.Value = value;
                            }
                        }

                        var dataRange = worksheet.Cells[1, 1, _dataViewForExport.Count + 1, columnsToExport];

                        dataRange.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        dataRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    loadTable();
                    new SuccesWindow("Данные успешно экспортированы!", Manager.theme).ShowDialog();
                }
                catch (Exception ex)
                {
                    new ErrorWindow($"Ошибка при экспорте: {ex.Message}", Manager.theme).ShowDialog();
                }
            }
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)new QuestionWindow("Вы уверены, что хотите удалить данного сотрудника?", Manager.theme).ShowDialog())
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

            if (!Manager.dataBase.completeCommand($"SET FOREIGN_KEY_CHECKS = 0; DELETE FROM `employee` WHERE `id`={dataFromRow[0]}; SET FOREIGN_KEY_CHECKS = 1;"))
            {
                _employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], $"Удалил сотрудника id {dataFromRow[0]} ({dataFromRow[4]})", DateTime.Now);

            loadTable();
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            new AddEmployee().ShowDialog();
            loadTable();
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            DataRowView rowView = button.DataContext as DataRowView;

            List<string> dataFromRow = new List<string>();
            foreach (DataColumn column in rowView.DataView.Table.Columns)
            {
                dataFromRow.Add(rowView[column.ColumnName].ToString());
            }

            new EditEmployee(dataFromRow).ShowDialog();
            loadTable();
        }
    }
}

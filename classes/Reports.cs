using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Provider.help_windows;
using System.IO;

namespace Provider.classes
{
    class Reports
    {
        public void GenerateWalletReplenishmentReport(DateTime startDate, DateTime endDate)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"WalletReplenishment_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                Title = "Сохранить файл Excel"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Dmitry");
                    using (var package = new ExcelPackage())
                    {
                        var reportData = GetReplenishmentData(startDate, endDate);

                        var worksheet = package.Workbook.Worksheets.Add("Пополнения счетов");

                        worksheet.Cells.Style.Font.Name = "Calibri";
                        worksheet.Cells.Style.Font.Size = 12;

                        worksheet.Cells[1, 1].Value = "Способ пополнения";
                        worksheet.Cells[1, 2].Value = "Телефон клиента";
                        worksheet.Cells[1, 3].Value = "Клиент";
                        worksheet.Cells[1, 4].Value = "Сотрудник";
                        worksheet.Cells[1, 5].Value = "Сумма (руб)";
                        worksheet.Cells[1, 6].Value = "Дата и время";

                        using (var range = worksheet.Cells[1, 1, 1, 6])
                        {
                            range.Style.Font.Size = 14;
                            range.Style.Font.Bold = true;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }

                        int row = 2;
                        decimal totalAmount = 0;

                        foreach (var item in reportData)
                        {
                            worksheet.Cells[row, 1].Value = item.ReplenishmentType;
                            worksheet.Cells[row, 2].Value = item.ClientPhone;
                            worksheet.Cells[row, 3].Value = item.ClientName;
                            worksheet.Cells[row, 4].Value = item.EmployeeName;
                            worksheet.Cells[row, 5].Value = item.Amount;
                            worksheet.Cells[row, 6].Value = item.TransactionDate.ToString("g");
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "dd.MM.yyyy HH:mm";

                            totalAmount += item.Amount;
                            row++;
                        }

                        worksheet.Cells[row, 4].Value = "Итого:";
                        worksheet.Cells[row, 5].Value = totalAmount;
                        worksheet.Cells[row, 4, row, 5].Style.Font.Bold = true;
                        worksheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        using (var range = worksheet.Cells[1, 1, row, 6])
                        {
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        }

                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    new SuccesWindow("Отчёт успешно создан!", Manager.theme).ShowDialog();
                }
                catch (Exception ex)
                {
                    new ErrorWindow($"Ошибка при экспорте: {ex.Message}", Manager.theme).ShowDialog();
                }
            }
        }

        private List<ReplenishmentData> GetReplenishmentData(DateTime startDate, DateTime endDate)
        {
            var result = new List<ReplenishmentData>();

            var clientReplenishments = Manager.dataBase.GetClientReplenishments(startDate, endDate);
            result.AddRange(clientReplenishments);

            var employeeReplenishments = Manager.dataBase.GetEmployeeReplenishments(startDate, endDate);
            result.AddRange(employeeReplenishments);

            return result.OrderBy(x => x.TransactionDate).ToList();
        }
    }
}

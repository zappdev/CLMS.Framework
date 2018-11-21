using System;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OfficeOpenXml.Style;

namespace CLMS.Framework.Utilities
{
    public class ExcelTools
    {
        public class ExportExcelOptions
        {
            public List<object> Columns { get; set; }
            public List<List<object>> Values { get; set; }
            public List<string> ColumnFormattings { get; set; }

            public string Title { get; set; }
            public string Author { get; set; }
            public string Path { get; set; }
              
            public bool IsValid => Columns != null && Values != null && Path != null;

            public FileInfo FileInfo
            {
                get
                {
                    if (Path == null) return null;

                    var newFile = new FileInfo(Path);
                    if (!newFile.Exists) return newFile;

                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(Path);
                    return newFile;
                }
            }
        }

        public static void ExportExcelFile(ExportExcelOptions options)
        {
            if (EnsureExcelOptionsAreValid(options)) throw new ExcelExportException();

            using (var package = new ExcelPackage(options.FileInfo))
            {
                using (var worksheet = package.Workbook.Worksheets.Add("Sheet1"))
				{

					// Add headers
					for (var columnIndex = 0; columnIndex < options.Columns.Count; columnIndex++)
					{	
						worksheet.Cells[1, columnIndex + 1].Value = options.Columns[columnIndex];
					}

				    worksheet.Cells[1, 1, 1, options.Columns.Count].Style.Font.Bold = true;
				    worksheet.Cells[1, 1, 1, options.Columns.Count].Style.Fill.PatternType = ExcelFillStyle.Solid;
				    worksheet.Cells[1, 1, 1, options.Columns.Count].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 221, 235, 247));
				    worksheet.Cells[1, 1, 1, options.Columns.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				    worksheet.Cells[1, 1, 1, options.Columns.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Add values
                    for (var rowIndex = 0; rowIndex < options.Values.Count; rowIndex++)
					{
						var row = options.Values[rowIndex];

						for (var colIndex = 0; colIndex < row.Count; colIndex++)
						{
							var cellValue = GetCellValue(row[colIndex]);
						    var columnFormatting = options.ColumnFormattings != null && options.ColumnFormattings.Count > colIndex ? options.ColumnFormattings[colIndex] : null;
                            var cell = worksheet.Cells[rowIndex + 2, colIndex + 1];
							SetCellFormat(options, cell, cellValue, columnFormatting);
							cell.Value = cellValue;
						}
					}

					worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
					// calculate
				    //worksheet.Calculate(); //not needed since no formulas are supported by our library

					// set some document properties
					package.Workbook.Properties.Title = options.Title;
					package.Workbook.Properties.Author = options.Author;
					package.Workbook.Properties.Comments = "This report was generated by zAppDev";

					package.Save();
				}
            }
        }

        private static object GetCellValue(object cellValue)
        {
            if (cellValue == null) return null;

            if(cellValue is int || cellValue is decimal || cellValue is byte || cellValue is long || cellValue is double || cellValue is short || cellValue is float)
            {
                cellValue = double.Parse(cellValue.ToString());
            }
            else if (cellValue is DateTime)
            {
                var dateTime = (cellValue as DateTime?).GetValueOrDefault();

                cellValue = dateTime;
            }

            return cellValue;
        }

        public static void SetCellFormat(ExportExcelOptions options, ExcelRange range, object value, string columnFormatting)
        {
            if (!string.IsNullOrWhiteSpace(columnFormatting) && (value is double || value is DateTime))
            {
                var formatting = value is DateTime ? Common.ConvertMomentFormat(columnFormatting) : columnFormatting.Replace("'", "");

                range.Style.Numberformat.Format = formatting;
            }
        }

        public static bool EnsureExcelOptionsAreValid(ExportExcelOptions options)
        {
            return options == null || !(options.IsValid);
        }

        public static byte[] GetExcelFormat(List<object> columns, List<List<object>> data)
        {
            byte[] result = null;
            using (var package = new ExcelPackage())
            {
                using (var worksheet = package.Workbook.Worksheets.Add("Sheet1"))
				{
					
					// Add headers
					for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
					{
						worksheet.Cells[1, columnIndex + 1].Value = columns[columnIndex];
					}

					// Add values
					for (var rowIndex = 0; rowIndex < data.Count; rowIndex++)
					{
						for (var colIndex = 0; colIndex < data[rowIndex].Count; colIndex++)
						{
							worksheet.Cells[rowIndex + 2, colIndex + 1].Value = data[rowIndex][colIndex];
						}
					}

					// calculate
					worksheet.Calculate();

					// set some document properties
					package.Workbook.Properties.Title = "";
					package.Workbook.Properties.Author = "zAppDev";
					package.Workbook.Properties.Comments = "This report was generated by zAppDev";

					result = package.GetAsByteArray();
				}
            }

            return result;
        }
    }

    public class ExcelExportException : Exception
    {
    }
}

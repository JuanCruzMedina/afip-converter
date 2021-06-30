using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Conversor.Clases
{
    public class Exporter
    {
        #region Properties
        public string ErrorMessage { get; set; }
        public List<string> lstExportedFiles { get; set; }
        #endregion
        #region Constructors
        public Exporter()
        {
            lstExportedFiles = new List<string>();
        }
        #endregion
        #region Methods
        public (bool,string) ExportToTxt(string[] lines, string extractFolderPath, string fileName)
        {
            try
            {
                string extractPath = Path.Combine(extractFolderPath, fileName);
                if (File.Exists(extractPath)) File.Delete(extractPath);
                System.IO.File.WriteAllLines(extractPath, lines);
                lstExportedFiles.Add(extractPath);
                return (true,"OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return (false, ex.Message);
            }
        }
        public bool ExportToTxt(List<object> lines, string extractFolderPath, string fileName)
        {
            try
            {
                List<string> result = lines.Select(x=> string.Join("\t",x.ToString())).ToList(); 
                string extractPath = Path.Combine(extractFolderPath, fileName);
                if (File.Exists(extractPath)) File.Delete(extractPath);
                System.IO.File.WriteAllLines(extractPath, result.ToArray());
                lstExportedFiles.Add(extractPath);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public bool ExportToExcel(DataTable dt, string excelfilepath=null)
        {
            try
            {
                DataTableExporter.ToExcel(dt,excelfilepath);
                lstExportedFiles.Add(excelfilepath);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message; 
                return false;
            }
        }
        public (bool, string) ExportToExcel(List<object> lines, string excelfilepath = null)
        {
            try
            {
                DataTableExporter.ToExcel(Converter.ConvertListToDataTable(lines), excelfilepath);
                lstExportedFiles.Add(excelfilepath);
                return (true, "OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return (false, ex.Message);
            }
        }
        #endregion
    }
    internal static class DataTableExporter
    {
        /// <summary>
        /// Export DataTable to Excel file
        /// </summary>
        /// <param name="DataTable">Source DataTable</param>
        /// <param name="ExcelFilePath">Path to result file name</param>
        public static void ToExcel(this System.Data.DataTable DataTable, string ExcelFilePath = null)
        {
            try
            {
                int ColumnsCount;

                if (DataTable == null || (ColumnsCount = DataTable.Columns.Count) == 0)
                    throw new Exception("ExportToExcel: Null or empty input table!\n");

                // load excel, and create a new workbook
                Microsoft.Office.Interop.Excel.Application Excel = new Microsoft.Office.Interop.Excel.Application();
                Excel.Workbooks.Add();

                // single worksheet
                Microsoft.Office.Interop.Excel._Worksheet Worksheet = Excel.ActiveSheet;

                object[] Header = new object[ColumnsCount];

                // column headings               
                for (int i = 0; i < ColumnsCount; i++)
                    Header[i] = DataTable.Columns[i].ColumnName;

                Microsoft.Office.Interop.Excel.Range HeaderRange = Worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)(Worksheet.Cells[1, 1]), (Microsoft.Office.Interop.Excel.Range)(Worksheet.Cells[1, ColumnsCount]));
                HeaderRange.Value = Header;
                HeaderRange.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.LightGray);
                HeaderRange.Font.Bold = true;

                // DataCells
                int RowsCount = DataTable.Rows.Count;
                object[,] Cells = new object[RowsCount, ColumnsCount];

                for (int j = 0; j < RowsCount; j++)
                    for (int i = 0; i < ColumnsCount; i++)
                        Cells[j, i] = DataTable.Rows[j][i];

                Worksheet.get_Range((Microsoft.Office.Interop.Excel.Range)(Worksheet.Cells[2, 1]), (Microsoft.Office.Interop.Excel.Range)(Worksheet.Cells[RowsCount + 1, ColumnsCount])).Value = Cells;

                // check fielpath
                if (ExcelFilePath != null && ExcelFilePath != "")
                {
                    try
                    {
                        Worksheet.SaveAs(ExcelFilePath);
                        Excel.Quit();
                        //System.Windows.MessageBox.Show("Excel file saved!");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("ExportToExcel: Excel file could not be saved! Check filepath.\n"
                            + ex.Message);
                    }
                }
                else    // no filepath is given
                {
                    Excel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ExportToExcel: \n" + ex.Message);
            }
        }
    }

}

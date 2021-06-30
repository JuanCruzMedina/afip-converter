using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Conversor.Clases;
using System.IO;
using static Conversor.Classes.Extractor;
using System.Linq;
using static Conversor.Clases.Controller;
using Conversor.Classes;
using PruebaState.Estados;

namespace Conversor
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            InitializeForm();
            this.lstBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox_DragDrop);
            this.lstBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox_DragEnter);
        }

        #region Properties
        public string FilePath { get; set; }
        public string ExtractionPath { get; set; }
        public bool ExtractionFlag { get; set; }
        public bool TxtFlag { get; set; }
        public bool XlsxFlag { get; set; }

        List<IExtraction> lstExtraccion = new List<IExtraction>();
        List<(string descripcion, int[] links, string separador)> lstLinks = new List<(string descripcion, int[] links, string separador)>();
        #endregion

        #region Events
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            SetFilePath(GetFilePath());
        }
        private void btnBorrarRuta_Click(object sender, EventArgs e)
        {
            ShowPath(false);
        }
        private void listBox_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }
        private void listBox_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            string[] s = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (System.IO.Path.GetExtension(s[0]) != "")
            {
                SetFilePath(s[0]);
            }
        }
        private void btnSalir_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btnEjecutar_Click(object sender, EventArgs e)
        {
            CorrectFile(CorrectTo.Export);
        }
        private void frmPrincipal_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("¿Está seguro que desea salir?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                e.Cancel = true;
        }
        private void btnBuscarRutaExtraccion_Click(object sender, EventArgs e)
        {
            lblRutaDefault.Text = GetFolderPath();
        }
        private void chkRutaDefault_CheckedChanged(object sender, EventArgs e)
        {
            pRutaExtraccion.Enabled = !chkRutaDefault.Checked;
            if (!chkRutaDefault.Checked) lblRutaDefault.Text = string.Empty;
            ExtractionFlag = chkRutaDefault.Checked;
        }
        private void chkTxt_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkTxt.Checked && !chkXlsx.Checked) chkTxt.Checked = true;
            TxtFlag = chkTxt.Checked;
        }
        private void chkXlsx_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkTxt.Checked && !chkXlsx.Checked) chkXlsx.Checked = true;
            XlsxFlag = chkXlsx.Checked;
        }
        private void btnResetConfiguration_Click(object sender, EventArgs e)
        {
            ResetConfig();
        }
        private void btnPrevisualizar_Click(object sender, EventArgs e)
        {
            CorrectFile(CorrectTo.Previsualize);
        }
        #endregion

        #region Private Methods

        private void InitializeForm()
        {
            CleanFields();
            ResetConfig();
            SetExtractions();
            SetLinks();
        }
        private void SetExtractions()
        {
            lstExtraccion = new List<IExtraction>
            {
                    new Extraction<int>(0, 4, "Año",0),
                    new Extraction<int>(4, 2, "Mes",1),
                    new Extraction<int>(6, 2, "Día",2),
                    new Extraction<int>(11, 5, "sucursal",3),
                    new Extraction<int>(28, 8, "Factura",4),
                    new Extraction<string>(78, 30, "Nombre",5),
                    new Extraction<decimal>(108, 15, "Importe",6)
            };
            dgvExtraciones.DataSource = lstExtraccion;
        }
        private void SetLinks()
        {
            lstLinks = new List<(string descripcion, int[] links, string separador)>()
            {
                (descripcion:"Fecha", new int[] { 2,1,0 }, "-"),
                (descripcion:"Sucursal", new int[] {4, }, ""),
                (descripcion:"Factura", new int[] {3 }, ""),
                (descripcion:"Cliente", new int[] { 5 }, "-"),
                (descripcion:"Importe", new int[] {6 }, "")
            };
        }
        private void CleanFields()
        {
            FilePath = string.Empty; ShowPath(false);
        }
        private void ResetConfig()
        {
            chkTxt.Checked = chkXlsx.Checked = chkRutaDefault.Checked = true;
            TxtFlag = XlsxFlag = ExtractionFlag = true;
            lblRutaDefault.Text = string.Empty;
        }

        private void CorrectFile(CorrectTo operation)
        {
            var context = new ProcesoContext(new Preparado(), FilePath, lblRutaDefault.Text, ExtractionFlag, XlsxFlag, TxtFlag);           
            do
            {
                context.Next();
            } while (!context.fin);

            if (context.value)
            {
                if (MessageBox.Show("Se realizó la corrección del archivo de forma exitosa.\n¿Desea Abrir los archivos?", "Confirmación", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    new Exporter().lstExportedFiles.ForEach(x => OpenFile(x));
            }

            else
                ShowMessage("Ocurrió una inconsistencia durante la corrección del archivo.\n" + context.mensaje);
        }
        private bool DoOperations(ref ProcessResult oResult, ref Exporter oExporter, List<Converter.Afip_Line> lstAfipLines, CorrectTo operation, string extractionPath)
        {
            //var oConverter = new Converter();

            //if (!oConverter.CorrectToAFIPLines(FilePath, ref lstAfipLines))
            //    oResult.AddMessage("(Proceso de Correción):\t" + oConverter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!TxtFlag || oExporter.ExportToTxt(Converter.ToArrayOfStrings(lstAfipLines), extractionPath, GenerateName(FilePath, Extension.Txt)))))
            //    oResult.AddMessage("(Proceso de exportación Txt):\t" + oExporter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!XlsxFlag || oExporter.ExportToExcel(Converter.ConvertListAFIPToDataTable(lstAfipLines), Path.Combine(extractionPath, GenerateName(FilePath, Extension.Xlsx))))))
            //    oResult.AddMessage("(Proceso de exportación Excel):\t" + oExporter.ErrorMessage);

            //else if (oResult.Message == null)
            //    oResult.Value = true;

            //return oResult.Value;
            return false;
        }
        private bool DoOperationsV2(ref ProcessResult oResult, ref Exporter oExporter, CorrectTo operation, string extractionPath)
        {
            var oConverter = new Converter();
            var lst = new List<object>();

            //if (!oConverter.Normalize(FilePath, ref lst, lstExtraccion, lstLinks))
            //    oResult.AddMessage("(Proceso de Correción):\t" + oConverter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!TxtFlag || oExporter.ExportToTxt(lst, extractionPath, GenerateName(FilePath, Extension.Txt)))))
            //    oResult.AddMessage("(Proceso de exportación Txt):\t" + oExporter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!XlsxFlag || oExporter.ExportToExcel(lst, Path.Combine(extractionPath, GenerateName(FilePath, Extension.Xlsx))))))
            //    oResult.AddMessage("(Proceso de exportación Excel):\t" + oExporter.ErrorMessage);

            //else if (oResult.Message == null)
            //    oResult.Value = true;

            //return oResult.Value;
            return false;
        }
        private bool DoOperationsV3( ref Exporter oExporter, CorrectTo process, string extractionPath)
        {
            var oConverter = new Converter();
            var lst = new List<object>();
            Operation operation = Operation.Normalize;
            string message = string.Empty;
            bool result = true;
            do
            {
                switch (operation)
                {
                    case Operation.Normalize:

                        break;

                    case Operation.Export_Txt:

                        break;

                    case Operation.Export_Excel:

                        break;

                    case Operation.Previsualize:

                        break;

                    case Operation.Finalize:

                        return true;

                    default:
                        return false;
                }
            } while (result);
            if (result)
            {

            }
            return result;
            //if (!oConverter.Normalize(FilePath, ref lst, lstExtraccion, lstLinks))
            //    oResult.AddMessage("(Proceso de Correción):\t" + oConverter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!TxtFlag || oExporter.ExportToTxt(lst, extractionPath, GenerateName(FilePath, Extension.Txt)))))
            //    oResult.AddMessage("(Proceso de exportación Txt):\t" + oExporter.ErrorMessage);

            //else if (!(CorrectTo.Export != operation || (!XlsxFlag || oExporter.ExportToExcel(lst, Path.Combine(extractionPath, GenerateName(FilePath, Extension.Xlsx))))))
            //    oResult.AddMessage("(Proceso de exportación Excel):\t" + oExporter.ErrorMessage);

            //else if (oResult.Message == null)
            //    oResult.Value = true;

            //return oResult.Value;
        }
        #region Paths Manipulation 
        public static bool OpenFile(string filePath)
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = filePath;
                p.Start();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        private string GetFilePath()
        {
            using (var opnDlg = new OpenFileDialog()) //ANY dialog
            {
                if (opnDlg.ShowDialog() == DialogResult.OK)
                    return opnDlg.FileName;
            }
            return string.Empty;
        }
        private string GetFolderPath()
        {
            using (var opnDlg = new FolderBrowserDialog()) //ANY dialog
            {
                if (opnDlg.ShowDialog() == DialogResult.OK)
                    return opnDlg.SelectedPath;
            }
            return string.Empty;
        }
        private void SetFilePath(string filePath)
        {
            if (PathIsValid(filePath))
            {
                FilePath = filePath; ShowPath();
            }
            else
            {
                FilePath = string.Empty; ShowPath(false);
            }
        }
        private bool PathIsValid(string filePath)
        {
            bool result = false;
            switch (System.IO.Path.GetExtension(filePath))
            {
                case (".txt"):
                    result = true;
                    break;
            }
            return result;
        }
        #endregion

        #region Visualizaciones
        private void ShowPath(bool show = true)
        {
            lblRuta.Visible = show;
            if (!show) FilePath = string.Empty;
            else lblRuta.Text = FilePath;
        }
        private void ShowMessage(string message, string title = "Información", MessageBoxIcon icon = MessageBoxIcon.Information, MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            MessageBox.Show(message, title, buttons, icon);
        }
        #endregion

        #region Carga de datos
        private void LoadListBox(List<string> lstStrings, ListBox listBox)
        {
            listBox.Items.Clear();
            lstStrings.ForEach(x => listBox.Items.Add(x));
        }
        private void LoadDataGridView(List<Clases.Converter.Afip_Line> lstAfipLines, DataGridView dataGridView)
        {
            dataGridView.DataSource = Converter.ConvertListAFIPToDataTable(lstAfipLines);
        }

        #endregion

        #region Extras
        private string GenerateName(string filePath, Extension extension)
        {
            string result = System.IO.Path.GetFileNameWithoutExtension(filePath) + "_" +
                             "corrected_" +
                             DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");

            switch (extension)
            {
                case Extension.Txt:
                    result += ".txt";
                    break;
                case Extension.Xls:
                    result += ".xls";
                    break;
                case Extension.Csv:
                    result += ".csv";
                    break;
                case Extension.Xlsx:
                    result += ".xlsx";
                    break;
            }
            return result;
        }
        #endregion

        #endregion

        #region Internal Classes
        internal class ProcessResult
        {
            public bool Value { get; set; }
            public string Message { get; set; }

            public ProcessResult() { Value = false; }

            public void AddMessage(string message)
            {
                Message = Message == string.Empty ? "-" + message : Message + "\n- " + message;
                Value = false;
            }
        }
        #endregion

        #region Enumerators
        public enum Extension
        {
            Txt = 0,
            Xls = 1,
            Csv = 2,
            Xlsx = 3
        }
        public enum CorrectTo
        {
            Export = 0,
            Previsualize = 1
        }
        public enum Operation
        {
            Normalize,
            Export_Txt,
            Export_Excel,
            Previsualize,
            Finalize
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Conversor.Clases
{
    public class Converter
    {

        #region Properties
        public string ErrorMessage { get; set; }
        #endregion

        #region Methods
        public bool CorrectToAFIPLines(string filePath, ref List<Afip_Line> LineasAfip)
        {
            try
            {
                using (StreamReader ReaderObject = new StreamReader(filePath))
                {
                    string Line;
                    while ((Line = ReaderObject.ReadLine()) != null)
                    {
                        LineasAfip.Add(new Afip_Line(Line));
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }
        public static List<string> ToListOfStrings(List<Afip_Line> LineasAfip)
        {
            var lst = new List<string>();
            LineasAfip.ForEach(x => lst.Add(x.ToString()));
            return lst;
        }
        public static String[] ToArrayOfStrings(List<Afip_Line> LineasAfip)
        {
            return ToListOfStrings(LineasAfip).ToArray();
        }
        public static DataTable ConvertListToDataTable<TItemType>(List<TItemType> list)
        {
            DataTable convertedData = new DataTable();

            // Get List Item Properties info
            Type itemType = typeof(TItemType);
            PropertyInfo[] publicProperties =
                // Only public non inherited properties
                itemType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Create Table Columns
            foreach (PropertyInfo property in publicProperties)
            {
                // DataSet does not support System.Nullable<>
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    // Set the column datatype as the nullable value type
                    convertedData.Columns.Add(property.Name, property.PropertyType.GetGenericArguments()[0]);
                }
                else
                {
                    convertedData.Columns.Add(property.Name, property.PropertyType);
                }
            }

            // Convert the Data
            foreach (TItemType item in list)
            {
                object[] rowData = new object[convertedData.Columns.Count];
                int rowDataIndex = 0;
                // Iterate through Item Properties
                foreach (PropertyInfo property in publicProperties)
                {
                    // Add a single cell data
                    rowData[rowDataIndex] = property.GetValue(item, null);
                    rowDataIndex++;
                }
                convertedData.Rows.Add(rowData);
            }

            return convertedData;
        }
        public static DataTable ConvertListAFIPToDataTable(List<Clases.Converter.Afip_Line> lstAfipLines)
        {
            var table = CreateAFIPDataTable();
            lstAfipLines.ForEach(x => { table.Rows.Add(x.Fecha, x.Sucursal, x.Factura, x.Nombre, x.Importe); });
            return table;
        }
        
        #endregion

        #region Structure Generators
        private static DataTable CreateAFIPDataTable()
        {
            var table = new DataTable();
            DataColumn column;

            column = new DataColumn
            {
                DataType = System.Type.GetType("System.DateTime"),
                ColumnName = "Fecha"
            };
            table.Columns.Add(column);

            column = new DataColumn
            {
                DataType = Type.GetType("System.Int32"),
                ColumnName = "Sucursal"
            };
            table.Columns.Add(column);

            column = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "Factura"
            };
            table.Columns.Add(column);

            column = new DataColumn
            {
                DataType = Type.GetType("System.String"),
                ColumnName = "Nombre"
            };
            table.Columns.Add(column);

            column = new DataColumn
            {
                DataType = Type.GetType("System.Double"),
                ColumnName = "Importe"
            };
            table.Columns.Add(column);

            return table;
        }
        #endregion

        #region Classes
        public class Afip_Line
        {
            #region Properties
            public DateTime Fecha { get; set; }
            private string año { get; set; }
            private string mes { get; set; }
            private string dia { get; set; }
            public int Sucursal { get; set; }
            public string Factura { get; set; }
            public string Nombre { get; set; }
            public string Importe { get; set; }
            private string ErrorMessage { get; set; }
            #endregion

            #region Constructors
            public Afip_Line() { }
            public Afip_Line(string line)
            {
                string aux = "";
                string charAcum = "";
                for (int i = 0; i < line.Count() - 1; i++)
                {
                    switch (i)
                    {
                        case 4:
                            año = charAcum; charAcum = "";
                            break;
                        case 6:
                            mes = charAcum; charAcum = "";
                            break;
                        case 8:
                            dia = charAcum; charAcum = "";
                            break;
                        case 11:
                            charAcum = "";
                            break;
                        case 16:
                            Sucursal = int.Parse(charAcum); charAcum = "";
                            break;
                        case 28:
                            charAcum = "";
                            break;
                        case 36:
                            Factura = int.Parse(charAcum).ToString(); charAcum = "";
                            break;
                        case 78:
                            charAcum = "";
                            break;
                        case 108:
                            charAcum.Split(' ').ToList().ForEach(x => Nombre = Nombre == string.Empty ? capitalizeFirstCharacter(x) : Nombre + " " + capitalizeFirstCharacter(x)); 
                            charAcum = "";
                            break;
                        case 121:
                            aux += charAcum + ","; charAcum = "";
                            break;
                        case 123:
                            Importe = aux + charAcum; charAcum = "";
                            break;
                    }
                    charAcum += line[i];
                    if (i > 124) break;
                }
                Fecha = DateTime.Parse(dia + "/" + mes + "/" + año);
            }
            #endregion

            #region Public Methods
            public override string ToString()
            {
                return Fecha.ToShortDateString() + "\t" +
                        Sucursal.ToString() + "\t" +
                        int.Parse(Factura).ToString() + "\t" +
                        Nombre.ToString() + "\t" +
                        double.Parse(Importe).ToString();
            }
            public string ToCSV()
            {
                return Fecha.ToShortDateString() + "," +
                        Sucursal + "," +
                        Factura + "," +
                        Nombre.ToString() + "," +
                        Importe.ToString();
            }
            public string GetErrorMessage()
            {
                return ErrorMessage;
            }
            #endregion

            #region Private Methods
            private string capitalizeFirstCharacter(string format)
            {
                if (string.IsNullOrEmpty(format))
                    return string.Empty;
                else
                    return char.ToUpper(format[0]) + format.ToLower().Substring(1);
            }
            #endregion
        }
        #endregion

        public (bool Result,string Message) Normalize(string filePath, ref List<Object> lstLines, List<Controller.IExtraction> lstExtractions,List<(string descripcion, int[] links, string separador)> Links)
        {
            try
            {
                string line;
                using (StreamReader ReaderObject = new StreamReader(filePath))
                {
                    while ((line = ReaderObject.ReadLine()) != null)
                    {
                        var result = Controller.GetValueOfExtractions<Controller.IExtraction>(line, lstExtractions, Links, true).Select(x => x.contenido).ToArray();
                        lstLines.Add(result);
                    }
                }
                return (true,"OK");
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return (false, ex.Message);
            }
        }
    }
}

using OfficeOpenXml;
using System.Reflection;

namespace Facturacion.API.Helpers
{
    public class ExcelFormater
    {
        static ExcelFormater()
        {
            // Set the LicenseContext before using EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // or LicenseContext.Commercial
        }

        public static byte[] ExporExcel<T>(List<T> list, string titulo)
        {
            //String imageFile = "C:/logo_benavides.jpg";
            try
            {
                using (var package = new ExcelPackage())
                {
                    var result = new List<string>();
                    var resultArray = new List<string[]>();

                    Type type = typeof(T);
                    PropertyInfo[] properties = type.GetProperties();

                    // Add header with property names
                    var header = string.Join(",", Array.ConvertAll(properties, p => p.Name));
                    var headerArray = header.Split(',');

                    result.Add(header);
                    resultArray.Add(headerArray);

                    // Add each object's property values
                    foreach (var item in list)
                    {
                        var values = Array.ConvertAll(properties, p => p.GetValue(item)?.ToString());
                        var row = string.Join(",", values);
                        result.Add(row);
                        resultArray.Add(row.Split(','));
                    }




                    //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    // Añade una hoja al paquete
                    var worksheet = package.Workbook.Worksheets.Add(titulo);

                    int i = 1;
                    int j = 1;

                    //colocamos el cabecero
                    foreach (var item in headerArray)
                    {
                        worksheet.Cells[i, j].Value = item;
                        j = j + 1;
                    }

                    i = i + 1;

                    //colocamos el contenido
                    foreach (var row in resultArray)
                    {
                        j = 1;

                        foreach (var item in row)
                        {
                            worksheet.Cells[i, j].Value = item;
                            j = j + 1;
                        }

                        i = i + 1;
                    }

                    // Convierte el paquete de Excel a un arreglo de bytes
                    var fileBytes = package.GetAsByteArray();

                    // Devuelve el arreglo de bytes como un FileResult
                    return fileBytes;// File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "example.xlsx");
                }


            }
            catch (Exception ioe)
            {
                return null;
            }

        }
    }
}

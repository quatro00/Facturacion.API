using CsvHelper;
using System.Data;
using System.Globalization;

namespace Facturacion.API.Helpers
{
    public class Helpers
    {
        /*
        public DataTable ConvertCsvToDataTable(IFormFile file)
        {
            using (var stream = new StreamReader(file.OpenReadStream()))
            using (var csvReader = new CsvReader(stream, CultureInfo.InvariantCulture))
            {
                DataTable dataTable = new DataTable();
                csvReader.Read();
                csvReader.ReadHeader();
                foreach (string header in csvReader.HeaderRecord)
                {
                    dataTable.Columns.Add(header);
                }
                while (csvReader.Read())
                {
                    DataRow row = dataTable.NewRow();
                    for (int i = 0; i < csvReader.Context.Record.Length; i++)
                    {
                        row[i] = csvReader.GetField(i);
                    }
                    dataTable.Rows.Add(row);
                }
                return dataTable;
            }
        }
        */
        public DataTable ConvertCsvToList(IFormFile file)
        {
            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // Do any configuration to `CsvReader` before creating CsvDataReader.
                using (var dr = new CsvDataReader(csv))
                {
                    var dt = new DataTable();
                    dt.Load(dr);
                    return dt;
                }
            }
        }
    }
}

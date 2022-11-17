using System.Globalization;

using CsvHelper;

using Iida.Core.CsvHelper;

namespace Iida.Api.CsvHelper;

public class CsvService : ICsvService {
	public IEnumerable<T> ReadCsv<T>(Stream file) {
		var reader = new StreamReader(file);
		var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
		var records = csv.GetRecords<T>();
		return records;
	}
}
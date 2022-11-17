namespace Iida.Core.CsvHelper;

public interface ICsvService {
	IEnumerable<T> ReadCsv<T>(Stream file);
}
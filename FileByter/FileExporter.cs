using System.Linq;
using System.Text;

namespace FileByter
{
	public class FileExporter<T>
	{
		private readonly FileExportSpecification<T> _exportSpecification;

		public FileExporter(FileExportSpecification<T> exportSpecification)
		{
			_exportSpecification = exportSpecification;
		}



		public string ReadItemIntoRow(T item)
		{
			var sb = new StringBuilder();
			var allPropertyValues = _exportSpecification.Properties.Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetValue(item);
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(_exportSpecification.ColumnDelimeter);
			}

			return sb.ToString();
		}
	}
}
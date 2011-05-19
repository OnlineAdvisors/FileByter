using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileByter
{
	public class FileExporter<T>
	{
		private readonly FileExportSpecification<T> _fileExportConfiguration;

		public FileExporter(FileExportSpecification<T> fileExportConfiguration)
		{
			_fileExportConfiguration = fileExportConfiguration;
		}

		private string ReadItemIntoRow(T item)
		{
			var sb = new StringBuilder();
			var allPropertyValues = _fileExportConfiguration.Properties.Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedValue(item);
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(_fileExportConfiguration.ColumnDelimeter);
			}

			return sb.ToString();
		}

		public void ExportToFile(IEnumerable<T> items, string filePath)
		{
			using (TextWriter fileStream = new StreamWriter(filePath))
			{
				ExportToStream(items, fileStream);
			}
		}

		public void ExportToStream(IEnumerable<T> items, TextWriter writer)
		{
			bool isFirstRow = true;
			foreach (var item in items)
			{
				if (!isFirstRow)
				{
					writer.Write(_fileExportConfiguration.RowDelimeter);
				}
				else
				{
					// Write any pre-header information
					var prePendValue = _fileExportConfiguration.PrePendFileWithValue;
					if (!string.IsNullOrEmpty(prePendValue))
					{
						writer.Write(prePendValue);
						writer.Write(_fileExportConfiguration.RowDelimeter);
					}

					// Write the Header row
					if (_fileExportConfiguration.IncludeHeader)
					{
						writer.Write(WriteTheHeader());
						writer.Write(_fileExportConfiguration.RowDelimeter);
					}
				}
				isFirstRow = false;

				string rowText = ReadItemIntoRow(item);
				writer.Write(rowText);
			}

		}

		private string WriteTheHeader()
		{
			var sb = new StringBuilder();
			var allPropertyValues = _fileExportConfiguration.Properties.Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedHeader();
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(_fileExportConfiguration.ColumnDelimeter);
			}

			return sb.ToString();
		}

		public string ExportToString(IEnumerable<T> items)
		{
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb))
			{
				ExportToStream(items, sw);
			}

			return sb.ToString();
		}
	}
}
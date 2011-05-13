using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileByter
{
	public class FileExporter<T>
	{
		private readonly FileExportConfiguration<T> _fileExportConfiguration;

		public FileExporter(FileExportConfiguration<T> fileExportConfiguration)
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

				string formattedValue = property.GetValue(item);
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
				isFirstRow = false;

				string rowText = ReadItemIntoRow(item);
				writer.Write(rowText);
			}

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
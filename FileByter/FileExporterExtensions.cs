using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileByter
{
	public static class FileExporterExtensions
	{
		private static string WriteTheHeader<T>(FileExportSpecification spec)
		{
			var sb = new StringBuilder();
			var allPropertyValues = spec.GetPropertiesForType<T>().Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedHeader();
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(spec.ColumnDelimeter);
			}

			return sb.ToString();
		}
		private static string ReadItemIntoRow<T>(FileExportSpecification spec, T item)
		{
			var columnDelimeter = spec.ColumnDelimeter;
			var sb = new StringBuilder();
			var allPropertyValues = spec.GetPropertiesForType<T>().Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedValue(item);

				if (formattedValue.Contains(columnDelimeter))
					formattedValue = spec.OnDelimeterFoundInValue(property.PropertyName, columnDelimeter, formattedValue);

				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(columnDelimeter);
			}

			return sb.ToString();
		}

		public static void ExportToStream<T>(this FileExportSpecification spec, IEnumerable<T> items, TextWriter writer)
		{
			bool isFirstRow = true;
			foreach (T item in items)
			{
				if (!isFirstRow)
				{
					writer.Write(spec.RowDelimeter);
				}
				else
				{
					//TODO: Not happy with the API for prepending and appending to a file

					// Write any pre-header information
					var prePendValue = spec.PrePendFileWithValue;
					if (!string.IsNullOrEmpty(prePendValue))
					{
						writer.Write(prePendValue);
						writer.Write(spec.RowDelimeter);
					}

					// Write the Header row
					if (spec.IncludeHeader)
					{
						writer.Write(WriteTheHeader<T>(spec));
						writer.Write(spec.RowDelimeter);
					}
				}
				isFirstRow = false;

				string rowText = ReadItemIntoRow(spec, item);
				writer.Write(rowText);
			}

			writer.Write(spec.RowDelimeter);

			//TODO: Not happy with the API for prepending and appending to a file
			var apendValue = spec.AppendFileWithValue;
			if (!string.IsNullOrEmpty(apendValue))
			{
				writer.Write(apendValue);
			}
		}


		public static string ExportToString<T>(this FileExportSpecification spec, IEnumerable<T> items)
		{
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb))
			{
				ExportToStream(spec, items, sw);
			}

			return sb.ToString();
		}

		public static void ExportToFile<T>(this FileExportSpecification spec, IEnumerable<T> items, string filePath)
		{
			using (TextWriter fileStream = new StreamWriter(filePath))
			{
				ExportToStream(spec, items, fileStream);
			}
		}

	}
}
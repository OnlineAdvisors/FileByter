using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileByter
{
	public static class FileExporterExtensions
	{
		private static string WriteTheHeader<T>(FileExportSpecification<T> spec)
		{
			var sb = new StringBuilder();
			var allPropertyValues = spec.Properties.Values.ToList();
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
		private static string ReadItemIntoRow<T>(FileExportSpecification<T> spec, T item)
		{
			var sb = new StringBuilder();
			var allPropertyValues = spec.Properties.Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedValue(item);
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(spec.ColumnDelimeter);
			}

			return sb.ToString();
		}

		public static void ExportToStream<T>(this FileExportSpecification<T> spec, IEnumerable<T> items, TextWriter writer)
		{
			bool isFirstRow = true;
			foreach (var item in items)
			{
				if (!isFirstRow)
				{
					writer.Write(spec.RowDelimeter);
				}
				else
				{
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
						writer.Write(WriteTheHeader(spec));
						writer.Write(spec.RowDelimeter);
					}
				}
				isFirstRow = false;

				string rowText = ReadItemIntoRow(spec, item);
				writer.Write(rowText);
			}

		}


		public static string ExportToString<T>(this FileExportSpecification<T> spec, IEnumerable<T> items)
		{
			var sb = new StringBuilder();
			using (var sw = new StringWriter(sb))
			{
				ExportToStream(spec, items, sw);
			}

			return sb.ToString();
		}

		public static void ExportToFile<T>(this FileExportSpecification<T> spec, IEnumerable<T> items, string filePath)
		{
			using (TextWriter fileStream = new StreamWriter(filePath))
			{
				ExportToStream(spec, items, fileStream);
			}
		}

	}
}
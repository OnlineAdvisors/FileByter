using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileByter
{
	public static class FileExporterExtensions
	{
		private static string WriteTheHeader(Type type, FileExportSpecification spec)
		{
			var sb = new StringBuilder();
			var allPropertyValues = spec.GetPropertiesForType(type).Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedHeader();
				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(spec.ColumnDelimiter);
			}

			return sb.ToString();
		}
		private static string ReadItemIntoRow(FileExportSpecification spec, object item)
		{
			var columnDelimiter = spec.ColumnDelimiter;
			var sb = new StringBuilder();
			var allPropertyValues = spec.GetPropertiesForType(item.GetType()).Values.ToList();
			for (var i = 0; i < allPropertyValues.Count; i++)
			{
				var property = allPropertyValues[i];

				string formattedValue = property.GetFormattedValue(item);

				if (formattedValue.Contains(columnDelimiter))
					formattedValue = spec.OnDelimiterFoundInValue(property.PropertyName, columnDelimiter, formattedValue);

				sb.Append(formattedValue);

				if (i < (allPropertyValues.Count - 1))
					sb.Append(columnDelimiter);
			}

			return sb.ToString();
		}

		public static void ExportToStream(this FileExportSpecification spec, IEnumerable items, TextWriter writer)
		{
			bool isFirstRow = true;
			foreach (object item in items)
			{
				if (!isFirstRow)
				{
					writer.Write(spec.RowDelimiter);
				}
				else
				{
					//TODO: Not happy with the API for prepending and appending to a file

					// Write any pre-header information
					var prePendValue = spec.PrePendFileWithValue;
					if (!string.IsNullOrEmpty(prePendValue))
					{
						writer.Write(prePendValue);
						writer.Write(spec.RowDelimiter);
					}

					// Write the Header row
					if (spec.IncludeHeader)
					{
						writer.Write(WriteTheHeader(item.GetType(), spec));
						writer.Write(spec.RowDelimiter);
					}
				}
				isFirstRow = false;

				string rowText = ReadItemIntoRow(spec, item);
				writer.Write(rowText);
			}

			writer.Write(spec.RowDelimiter);

			//TODO: Not happy with the API for prepending and appending to a file
			var apendValue = spec.AppendFileWithValue;
			if (!string.IsNullOrEmpty(apendValue))
			{
				writer.Write(apendValue);
			}
		}


		public static string ExportToString(this FileExportSpecification spec, IEnumerable items)
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
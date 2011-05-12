﻿using System.Collections.Generic;
using System.IO;

namespace FileByter
{
	public static class FileExportExtensions
	{
		public static void ExportToFile<T>(this IEnumerable<T> items, string filePath, FileExportSpecification<T> fileExportSpecification)
		{
			var fileExporter = new FileExporter<T>(fileExportSpecification);

			using (TextWriter fileStream = new StreamWriter(filePath))
			{
				bool isFirstRow = true;
				foreach (var item in items)
				{
					if (!isFirstRow)
					{
						fileStream.Write(fileExportSpecification.RowDelimeter);
					}
					isFirstRow = false;

					string rowText = fileExporter.ReadItemIntoRow(item);
					fileStream.Write(rowText);
				}
			}
		}
	}
}
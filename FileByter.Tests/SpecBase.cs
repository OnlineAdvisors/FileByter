using System;
using System.Collections.Generic;

namespace FileByter.Tests
{
	public class SpecBase
	{
		protected static string GetExportResult<T>(IEnumerable<T> items, Action<FileExportSpecification<T>> config)
		{
			return new FileExportSpecificationFactory<T>()
				.CreateSpec(config)
				.CreateFileExporter()
				.ExportToString(items);
		}
	}
}
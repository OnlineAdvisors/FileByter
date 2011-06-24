using System;
using System.Collections.Generic;

namespace FileByter.Tests
{
	public class SpecBase
	{
		protected static string GetExportResult<T>(IEnumerable<T> items, Action<FileExportSpecification> config)
		{
			return new FileExport<T>()
				.CreateSpec(config)
				.ExportToString(items);
		}
	}
}
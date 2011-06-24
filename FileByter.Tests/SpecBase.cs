using System;
using System.Collections.Generic;
using System.Linq;

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

		protected static string GetExportResult(IEnumerable<object> items, Action<FileExportSpecification> config)
		{
			var distinctTypes = items.Select(s => s.GetType()).Distinct();

			return new FileExport()
				.CreateSpec(distinctTypes, config)
				.ExportToString(items);
		}

	}
}
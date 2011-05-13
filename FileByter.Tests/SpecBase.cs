using System;
using System.Collections.Generic;

namespace FileByter.Tests
{
    public class SpecBase
    {
        protected static string GetExportResult<T>(IEnumerable<T> items, Action<FileExportSpecification<T>> config)
        {
            var factory = new FileExportSpecificationFactory<T>();
            var fileExportSpecification = factory.CreateSpec(config);
            var fileExporter = factory.CreateFileExporter(fileExportSpecification);

            return fileExporter.ExportToString(items);
        }
    }
}
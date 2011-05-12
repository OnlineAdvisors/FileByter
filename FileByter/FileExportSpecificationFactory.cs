using System;
using System.Collections.Generic;

namespace FileByter
{
	public class FileExportSpecificationFactory
	{
		public FileExportSpecification<T> Create<T>()
		{
			return Create<T>(cfg => { });
		}

		public FileExportSpecification<T> Create<T>(Action<FileExportSpecification<T>> configuration)
		{
			var fileExportSpecification = new FileExportSpecification<T>();
			configuration(fileExportSpecification);

			ConfigureRestOfProperties(fileExportSpecification);
			return fileExportSpecification;
		}

		private void ConfigureRestOfProperties<T>(FileExportSpecification<T> fileExportSpecification)
		{
			// fallback formatter for anything that doesn't fit int he custom, or "global default" formatters.
			var globalDefaultFormatter = new PropertyFormatter(propertyValue =>
																{
																	if (propertyValue == null)
																	{
																		return string.Empty;
																	}
																	return propertyValue.ToString();
																});

			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				if (fileExportSpecification.IsPropertyDefined(propertyInfo.Name))
					continue;

				PropertyFormatter defaultPropertyFormatter = globalDefaultFormatter;

				// If there's a default 
				if (_defaultTypeFormatters.ContainsKey(propertyInfo.PropertyType))
				{
					defaultPropertyFormatter = _defaultTypeFormatters[propertyInfo.PropertyType];
				}

				fileExportSpecification.Add(propertyInfo.Name, defaultPropertyFormatter);
			}
		}

		private readonly IDictionary<Type, PropertyFormatter> _defaultTypeFormatters = new Dictionary<Type, PropertyFormatter>();
		public void AddDefault<T>(PropertyFormatter formatter)
		{
			_defaultTypeFormatters.Add(typeof(T), formatter);
		}

	}

}
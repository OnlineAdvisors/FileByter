using System;
using System.Collections.Generic;

namespace FileByter
{
	public class FileExport<T>
	{
		/// <summary>
		/// Create a <seealso cref="FileExportSpecification&lt;T&gt;" /> with the default configuration.
		/// </summary>
		/// <returns></returns>
		public FileExportSpecification<T> CreateSpec()
		{
			return CreateSpec(cfg => { });
		}

		/// <summary>
		/// Create a <seealso cref="FileExportSpecification&lt;T&gt;" /> by giving the option for custom configuration.
		/// </summary>
		public FileExportSpecification<T> CreateSpec(Action<FileExportSpecification<T>> configuration)
		{
			var fileExportSpecification = new FileExportSpecification<T>(this);
			configuration(fileExportSpecification);

			if (!fileExportSpecification.SkipRestOfProperties)
			{
				ConfigureRestOfProperties(fileExportSpecification);
			}

			return fileExportSpecification;
		}

		private void ConfigureRestOfProperties(FileExportSpecification<T> fileExportSpecification)
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
			var properties = typeof(T).GetProperties();

			for (int i = 0; i < properties.Length; i++)
			{
				var propertyInfo = properties[i];
				var propertyName = propertyInfo.Name;

				if (!fileExportSpecification.IsPropertyExcluded(propertyName))
				{
					if (fileExportSpecification.IsPropertyDefined(propertyName))
					{
						fileExportSpecification[propertyName].Order = i;
						continue;
					}

					PropertyFormatter defaultPropertyFormatter = globalDefaultFormatter;

					// If there's a default 
					if (_defaultTypeFormatters.ContainsKey(propertyInfo.PropertyType))
					{
						defaultPropertyFormatter = _defaultTypeFormatters[propertyInfo.PropertyType];
					}

					fileExportSpecification.AddPropertyFormatter(propertyName, defaultPropertyFormatter, i);

				}
			}
		}

		private readonly IDictionary<Type, PropertyFormatter> _defaultTypeFormatters = new Dictionary<Type, PropertyFormatter>();
		public void AddDefault<TProperty>(Func<TProperty, string> formatter)
		{
			PropertyFormatter pf = item => formatter((TProperty)item);
			_defaultTypeFormatters.Add(typeof(TProperty), pf);
		}

		public FileExporter<TInput> CreateFileExporter<TInput>(FileExportSpecification<TInput> fileExportSpecification)
		{
			PropertiesCollection<TInput> properties = fileExportSpecification.Properties;
			string columnDelimeter = fileExportSpecification.ColumnDelimeter;
			var fileExportConfiguration = new FileExportConfiguration<TInput>(properties, columnDelimeter);
			var fileExporter = new FileExporter<TInput>(fileExportConfiguration);
			return fileExporter;
		}
	}

}
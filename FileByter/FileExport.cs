using System;
using System.Collections.Generic;
using System.Linq;

namespace FileByter
{
	public class FileExport
	{

	}
	public class FileExport<T> : FileExport
	{
		private readonly IDictionary<Type, PropertyFormatter<T>> _defaultTypeFormatters = new Dictionary<Type, PropertyFormatter<T>>();

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
			var fileExportSpecification = new FileExportSpecification<T>();
			configuration(fileExportSpecification);

			if (!fileExportSpecification.SkipNonConfiguredProperties)
			{
				ConfigureRestOfProperties(fileExportSpecification);
			}

			return fileExportSpecification;
		}

		private void ConfigureRestOfProperties(FileExportSpecification<T> fileExportSpecification)
		{
			// fallback formatter for anything that doesn't fit int he custom, or "global default" formatters.
			var globalDefaultFormatter = new PropertyFormatter<T>(context =>
																{
																	if (context.ReadValue == null)
																	{
																		return string.Empty;
																	}
																	return context.ReadValue.ToString();
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

					PropertyFormatter<T> defaultPropertyFormatter = globalDefaultFormatter;

					// If there's a default
					if (_defaultTypeFormatters.ContainsKey(propertyInfo.PropertyType))
					{
						defaultPropertyFormatter = _defaultTypeFormatters[propertyInfo.PropertyType];
					}

					var property = new Property<T>(propertyName, defaultPropertyFormatter, fileExportSpecification.DefaultHeaderFormatter, i);

					fileExportSpecification.AddProperty(property);
				}
			}
		}

		public FileExport<T> AddDefault<TProperty>(Func<TProperty, string> formatter)
		{
			PropertyFormatter<T> pf = (context) => formatter((TProperty)context.ReadValue);
			_defaultTypeFormatters.Add(typeof(TProperty), pf);
			return this;
		}
	}

}
using System;
using System.Collections.Generic;

namespace FileByter
{
	public class FileExport
	{
		private readonly Type _rowType;
		protected readonly IDictionary<Type, IDictionary<Type, PropertyFormatter>> _defaultTypeFormatters = new Dictionary<Type, IDictionary<Type, PropertyFormatter>>();

		public FileExport(Type rowType)
		{
			_rowType = rowType;
			_defaultTypeFormatters.Add(_rowType, new Dictionary<Type, PropertyFormatter>());
		}
	}


	public class FileExport<T> : FileExport
	{
		public FileExport()
			: base(typeof(T))
		{
		}

		/// <summary>
		/// Create a <seealso cref="FileExportSpecification&lt;T&gt;" /> with the default configuration.
		/// </summary>
		/// <returns></returns>
		public FileExportSpecification CreateSpec()
		{
			return CreateSpec(cfg => { });
		}

		/// <summary>
		/// Create a <seealso cref="FileExportSpecification&lt;T&gt;" /> by giving the option for custom configuration.
		/// </summary>
		public FileExportSpecification CreateSpec(Action<FileExportSpecification> configuration)
		{
			var fileExportSpecification = new FileExportSpecification(new[] { typeof(T) }, ",", Environment.NewLine);
			configuration(fileExportSpecification);

			if (!fileExportSpecification.SkipNonConfiguredProperties)
			{
				ConfigureRestOfProperties(fileExportSpecification);
			}

			return fileExportSpecification;
		}

		private void ConfigureRestOfProperties(FileExportSpecification fileExportSpecification)
		{
			// fallback formatter for anything that doesn't fit int he custom, or "global default" formatters.
			var globalDefaultFormatter = new PropertyFormatter(context =>
																{
																	if (context.ItemValue == null)
																	{
																		return string.Empty;
																	}
																	return context.ItemValue.ToString();
																});
			Type objectType = typeof(T);
			var properties = objectType.GetProperties();

			for (int i = 0; i < properties.Length; i++)
			{
				var propertyInfo = properties[i];
				var propertyName = propertyInfo.Name;

				if (!fileExportSpecification.IsPropertyExcluded<T>(propertyName))
				{
					if (fileExportSpecification.IsPropertyDefined<T>(propertyName))
					{
						fileExportSpecification.GetPropertiesForType<T>()[propertyName].Order = i;
						continue;
					}

					PropertyFormatter defaultPropertyFormatter = globalDefaultFormatter;

					// If there's a default
					if (_defaultTypeFormatters[objectType].ContainsKey(propertyInfo.PropertyType))
					{
						defaultPropertyFormatter = _defaultTypeFormatters[objectType][propertyInfo.PropertyType];
					}

					var property = new Property(typeof(T), propertyName, defaultPropertyFormatter, fileExportSpecification.DefaultHeaderFormatter, i);

					fileExportSpecification.GetPropertiesForType<T>().AddProperty(propertyName, property);
				}
			}
		}

		public FileExport<T> AddDefault<TProperty>(Func<TProperty, string> formatter)
		{
			PropertyFormatter pf = context => formatter((TProperty)context.ItemValue);
			_defaultTypeFormatters[typeof(T)].Add(typeof(TProperty), pf);
			return this;
		}
	}

}
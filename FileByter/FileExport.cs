using System;
using System.Collections.Generic;

namespace FileByter
{
	public class FileExport
	{
		protected readonly Dictionary<Type, PropertyFormatter> DefaultPropertyFormatters = new Dictionary<Type, PropertyFormatter>();

		public FileExportSpecification CreateSpec(IEnumerable<Type> supportedTypes, Action<FileExportSpecification> configuration)
		{
			var fileExportSpecification = new FileExportSpecification(supportedTypes, ",", Environment.NewLine);
			configuration(fileExportSpecification);

			foreach (var supportedType in supportedTypes)
			{
				if (!fileExportSpecification.SkipNonConfiguredProperties)
				{
					ConfigureRestOfProperties(supportedType, fileExportSpecification);
				}
			}
			return fileExportSpecification;
		}

		private void ConfigureRestOfProperties(Type objectType, FileExportSpecification fileExportSpecification)
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
			var properties = objectType.GetProperties();

			for (int i = 0; i < properties.Length; i++)
			{
				var propertyInfo = properties[i];
				var propertyName = propertyInfo.Name;

				TypeConfiguration props = fileExportSpecification.GetTypeConfiguration(objectType);

				if (!props.IsPropertyExcluded(propertyName))
				{
					if (props.IsPropertyDefined(propertyName))
					{
						props.Properties[propertyName].Order = i;
						continue;
					}

					var propertyType = propertyInfo.PropertyType;

					PropertyFormatter defaultPropertyFormatter = globalDefaultFormatter;

					if (DefaultPropertyFormatters.ContainsKey(propertyType))
					{
						defaultPropertyFormatter = DefaultPropertyFormatters[propertyType];
					}

					if (fileExportSpecification.DefaultPropertyFormatters.ContainsKey(propertyType))
					{
						defaultPropertyFormatter = fileExportSpecification.DefaultPropertyFormatters[propertyType];
					}

					// If there's a default
					if (props.DefaultTypeFormatters.ContainsKey(propertyType))
					{
						defaultPropertyFormatter = props.DefaultTypeFormatters[propertyType];
					}

					var property = new Property(objectType, propertyName, defaultPropertyFormatter, fileExportSpecification.DefaultHeaderFormatter, i);

					props.AddProperty(property);
				}
			}
		}
	}


	public class FileExport<T> : FileExport
	{
		public FileExportSpecification CreateSpec()
		{
			return this.CreateSpec(cfg => { });
		}

		public FileExportSpecification CreateSpec(Action<FileExportSpecification> configuration)
		{
			var spec = CreateSpec(new[] { typeof(T) }, configuration);
			foreach (var configuredType in spec.ConfiguredTypes)
			{
				foreach (var localItem in spec.DefaultPropertyFormatters)
				{
					configuredType.AddDefault(localItem.Key, localItem.Value);
				}

				foreach (var rowTypeConfiguration in DefaultPropertyFormatters)
				{
					if (!configuredType.DefaultTypeFormatters.Keys.Contains(rowTypeConfiguration.Key))
					{
						configuredType.AddDefault(rowTypeConfiguration.Key, rowTypeConfiguration.Value);
					}
				}
			}
			return spec;
		}

		public FileExport<T> AddDefault<TProperty>(Func<TProperty, string> formatter)
		{
			PropertyFormatter pf = context => formatter((TProperty)context.ItemValue);
			DefaultPropertyFormatters.Add(typeof(TProperty), pf);
			return this;
		}
	}
}
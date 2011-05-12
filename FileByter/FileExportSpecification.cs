using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FileByter
{
	public class FileExportSpecification<T>
	{
		private readonly PropertiesCollection<T> _properties = new PropertiesCollection<T>();

		public FileExportSpecification()
			: this(",", Environment.NewLine)
		{
		}

		public PropertiesCollection<T> Properties
		{
			get { return _properties; }
		}

		public FileExportSpecification(string columnDelimeter, string rowDelimeter)
		{
			ColumnDelimeter = columnDelimeter;
			RowDelimeter = rowDelimeter;
		}

		public FileExportSpecification<T> Add(string propertyName, PropertyFormatter formatter)
		{
			// Should only add property once
			if (Properties.ContainsPropertyName(propertyName))
				throw new ArgumentException("The property [{0}] has been specified.".FormatWith(propertyName));

			var propertyReader = new PropertyReader<T>(propertyName);
			var property = new Property<T>(propertyReader, formatter);
			Properties.AddProperty(propertyName, property);
			return this;
		}

		public FileExportSpecification<T> Add<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Func<TProperty, string> formatter)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			if (formatter == null) throw new ArgumentNullException("formatter");

			var propertyName = propertyExpression.GetMemberName();

			return Add(propertyName, item => formatter((TProperty)item));
		}

		public FileExportSpecification<T> Exclude<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			var propertyName = propertyExpression.GetMemberName();
			this.Properties.AddExclusion(propertyName);
			return this;
		}

		public string ColumnDelimeter { get; set; }
		public string RowDelimeter { get; set; }

		public Property<T> this[string propertyName]
		{
			get
			{
				if (Properties.ContainsPropertyName(propertyName))
					return Properties[propertyName];

				throw new ArgumentException("propertyName not found [{0}]".FormatWith(propertyName));
			}
		}

		public bool IsPropertyDefined(string propertyName)
		{
			if (Properties.ContainsExcludedProperty(propertyName))
				return true;
			return Properties.ContainsPropertyName(propertyName);
		}
	}

	public class FileExportSpecification
	{
		public static FileExportSpecification<T> Create<T>()
		{
			return Create<T>(cfg => { });
		}
		public static FileExportSpecification<T> Create<T>(Action<FileExportSpecification<T>> configuration)
		{
			var fileExportSpecification = new FileExportSpecification<T>();
			configuration(fileExportSpecification);

			ConfigureRestOfProperties(fileExportSpecification);
			return fileExportSpecification;
		}

		private static void ConfigureRestOfProperties<T>(FileExportSpecification<T> fileExportSpecification)
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
				if (DefaultTypeFormatters.ContainsKey(propertyInfo.PropertyType))
				{
					defaultPropertyFormatter = DefaultTypeFormatters[propertyInfo.PropertyType];
				}

				fileExportSpecification.Add(propertyInfo.Name, defaultPropertyFormatter);
			}
		}

		private static readonly IDictionary<Type, PropertyFormatter> DefaultTypeFormatters = new Dictionary<Type, PropertyFormatter>();
		public static void AddDefault<T>(PropertyFormatter formatter)
		{
			DefaultTypeFormatters.Add(typeof(T), formatter);
		}
	}
}
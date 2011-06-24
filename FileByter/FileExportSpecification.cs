using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FileByter
{
	public delegate string DelimeterFoundInValue(string propertyName, string columnDelimeter, string value);

	public class FileExportSpecification
	{

	}

	public class TypeConfiguration
	{
		private readonly PropertiesCollection _properties = new PropertiesCollection();
		private readonly Type _rowType;

		public TypeConfiguration(Type rowType)
		{
			_rowType = rowType;
		}

		public PropertiesCollection Properties { get { return _properties; } }
	}

	public class TypeConfiguration<T> : TypeConfiguration
	{
		public TypeConfiguration()
			: base(typeof(T))
		{
		}
	}

	public class FileExportSpecification<T> : FileExportSpecification
	{
		private readonly Dictionary<Type, TypeConfiguration> _rowTypeConfigurations = new Dictionary<Type, TypeConfiguration>();

		private readonly HeaderFormatter _defaultHeaderFormatter = pi => pi.Name;
		public HeaderFormatter DefaultHeaderFormatter { get { return _defaultHeaderFormatter; } }

		public FileExportSpecification()
			: this(columnDelimeter: ",",
					rowDelimeter: Environment.NewLine)
		{
			_rowTypeConfigurations.Add(typeof(T), new TypeConfiguration(typeof(T)));
		}

		public FileExportSpecification(string columnDelimeter, string rowDelimeter)
		{
			ColumnDelimeter = columnDelimeter;
			RowDelimeter = rowDelimeter;
			OnDelimeterFoundInValue = (string propertyName, string columnDelimeterX, string value) =>
			{
				throw new FileExportException
					("Item with propertyName[{0}] and value[{1}] contained column delimeter [{2}]"
						.FormatWith(propertyName, value, columnDelimeterX));
			};
		}

		public PropertiesCollection GetPropertiesForType<T>()
		{
			//TODO: dictionary.Contains & throw if type not configured.
			return _rowTypeConfigurations[typeof(T)].Properties;
		}

		public FileExportSpecification<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter propertyFormatter, HeaderFormatter headerFormatter)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			if (propertyFormatter == null) throw new ArgumentNullException("propertyFormatter");

			var propertyName = propertyExpression.GetMemberName();

			if (headerFormatter == null)
				headerFormatter = pi => pi.Name;

			var property = new Property(typeof(T), propertyName, propertyFormatter, headerFormatter);

			return AddProperty(property);
		}
		public FileExportSpecification<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter formatter)
		{
			return AddPropertyFormatter(propertyExpression, formatter, null);
		}

		public FileExportSpecification<T> AddProperty(Property property)
		{
			var propertyName = property.PropertyName;

			// Should only add property once
			if (GetPropertiesForType<T>().ContainsPropertyName(propertyName))
				throw new ArgumentException("The property [{0}] has been already been specified.".FormatWith(propertyName));

			GetPropertiesForType<T>().AddProperty(propertyName, property);
			return this;
		}

		public FileExportSpecification<T> Exclude<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			var propertyName = propertyExpression.GetMemberName();
			GetPropertiesForType<T>().AddExclusion(propertyName);
			return this;
		}

		public string ColumnDelimeter { get; set; }
		public string RowDelimeter { get; set; }

		internal Property this[string propertyName]
		{
			get
			{
				if (GetPropertiesForType<T>().ContainsPropertyName(propertyName))
					return GetPropertiesForType<T>()[propertyName];

				throw new ArgumentException("propertyName not found [{0}]".FormatWith(propertyName));
			}
		}

		public bool IsPropertyDefined(string propertyName)
		{
			if (GetPropertiesForType<T>().ContainsExcludedProperty(propertyName))
				return true;
			return GetPropertiesForType<T>().ContainsPropertyName(propertyName);
		}

		public bool IsPropertyExcluded(string propertyName)
		{
			return GetPropertiesForType<T>().IsExcluded(propertyName);
		}

		private bool _excludeNonConfiguredProperties;
		internal string PrePendFileWithValue { get; private set; }
		internal string AppendFileWithValue { get; private set; }
		internal bool SkipNonConfiguredProperties { get { return _excludeNonConfiguredProperties; } }
		public void ExcludeNonConfiguredProperties()
		{
			_excludeNonConfiguredProperties = true;

		}

		public bool IncludeHeader { get; set; }

		public DelimeterFoundInValue OnDelimeterFoundInValue { get; set; }

		public void PrependFileWith(string value)
		{
			PrePendFileWithValue = value;
		}

		public void AppendFileWith(string value)
		{
			AppendFileWithValue = value;
		}
	}
}
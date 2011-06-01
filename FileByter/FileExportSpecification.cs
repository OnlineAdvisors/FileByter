using System;
using System.Linq.Expressions;

namespace FileByter
{
	public class FileExportSpecification<T>
	{
		private readonly PropertiesCollection<T> _properties = new PropertiesCollection<T>();
		private readonly HeaderFormatter _defaultHeaderFormatter = pi => pi.Name;
		public HeaderFormatter DefaultHeaderFormatter { get { return _defaultHeaderFormatter; } }

		public FileExportSpecification()
			: this(columnDelimeter: ",",
					rowDelimeter: Environment.NewLine)
		{
		}

		public FileExportSpecification(string columnDelimeter, string rowDelimeter)
		{
			ColumnDelimeter = columnDelimeter;
			RowDelimeter = rowDelimeter;
		}

		public PropertiesCollection<T> Properties
		{
			get { return _properties; }
		}

		public FileExportSpecification<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter<T> propertyFormatter, HeaderFormatter headerFormatter)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			if (propertyFormatter == null) throw new ArgumentNullException("propertyFormatter");

			var propertyName = propertyExpression.GetMemberName();

			if (headerFormatter == null)
				headerFormatter = pi => pi.Name;

			var property = new Property<T>(propertyName, propertyFormatter, headerFormatter);

			return AddProperty(property);
		}
		public FileExportSpecification<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter<T> formatter)
		{
			return AddPropertyFormatter(propertyExpression, formatter, null);
		}

		public FileExportSpecification<T> AddProperty(Property<T> property)
		{
			var propertyName = property.PropertyName;

			// Should only add property once
			if (Properties.ContainsPropertyName(propertyName))
				throw new ArgumentException("The property [{0}] has been already been specified.".FormatWith(propertyName));

			Properties.AddProperty(propertyName, property);
			return this;
		}

		public FileExportSpecification<T> Exclude<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			var propertyName = propertyExpression.GetMemberName();
			Properties.AddExclusion(propertyName);
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

		public bool IsPropertyExcluded(string propertyName)
		{
			return Properties.IsExcluded(propertyName);
		}

		private bool _excludeNonConfiguredProperties;
		internal string PrePendFileWithValue { get; private set; }
		internal bool SkipNonConfiguredProperties { get { return _excludeNonConfiguredProperties; } }
		public void ExcludeNonConfiguredProperties()
		{
			_excludeNonConfiguredProperties = true;

		}

		public bool IncludeHeader { get; set; }

		public void PrependFileWith(string value)
		{
			PrePendFileWithValue = value;
		}
	}
}
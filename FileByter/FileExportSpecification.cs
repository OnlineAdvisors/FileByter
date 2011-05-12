using System;
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
	}
}
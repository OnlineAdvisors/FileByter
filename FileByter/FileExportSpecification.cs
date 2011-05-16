using System;
using System.Linq.Expressions;

namespace FileByter
{
	public class FileExportSpecification<T>
	{
		private readonly FileExportSpecificationFactory<T> _fileExportSpecificationFactory;
		private readonly PropertiesCollection<T> _properties = new PropertiesCollection<T>();

		public FileExportSpecification(FileExportSpecificationFactory<T> fileExportSpecificationFactory)
			: this(columnDelimeter: ",",
					rowDelimeter: Environment.NewLine)
		{
			_fileExportSpecificationFactory = fileExportSpecificationFactory;
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

		public FileExportSpecification<T> AddPropertyFormatter(string propertyName, PropertyFormatter formatter, int order)
		{
			// Should only add property once
			if (Properties.ContainsPropertyName(propertyName))
				throw new ArgumentException("The property [{0}] has been already been specified.".FormatWith(propertyName));

			var propertyReader = new PropertyReader<T>(propertyName);
			var property = new Property<T>(propertyReader, formatter, order);
			Properties.AddProperty(propertyName, property);
			return this;
		}

		public FileExportSpecification<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, Func<TProperty, string> formatter)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			if (formatter == null) throw new ArgumentNullException("formatter");

			var propertyName = propertyExpression.GetMemberName();

			return AddPropertyFormatter(propertyName, item => formatter((TProperty)item), 0);
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

		public FileExporter<T> CreateFileExporter()
		{
			return _fileExportSpecificationFactory.CreateFileExporter(this);
		}
	}
}
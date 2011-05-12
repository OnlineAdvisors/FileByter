using System;

namespace FileByter
{
	public class FileExportConfiguration<T>
	{
		private readonly PropertiesCollection<T> _properties;
		private readonly string _columnDelimeter;
		private readonly string _rowDelimeter;


		public FileExportConfiguration(PropertiesCollection<T> properties, string columnDelimeter = ",")
			: this(properties, columnDelimeter, rowDelimeter: Environment.NewLine)
		{
		}

		public FileExportConfiguration(PropertiesCollection<T> properties, string columnDelimeter, string rowDelimeter)
		{
			if (properties == null) throw new ArgumentNullException("properties");

			_properties = properties;
			_rowDelimeter = rowDelimeter;
			_columnDelimeter = columnDelimeter;
		}

		public PropertiesCollection<T> Properties
		{
			get { return _properties; }
		}

		public string ColumnDelimeter
		{
			get { return _columnDelimeter; }
		}

		public string RowDelimeter
		{
			get { return _rowDelimeter; }
		}
	}
}
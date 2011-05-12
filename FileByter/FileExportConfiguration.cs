using System;

namespace FileByter
{
	public class FileExportConfiguration<T>
	{
		private readonly PropertiesCollection<T> _properties;
		private readonly string _columnDelimeter;

		public FileExportConfiguration(PropertiesCollection<T> properties, string columnDelimeter = ",")
		{
			if (properties == null) throw new ArgumentNullException("properties");

			_properties = properties;
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
	}
}
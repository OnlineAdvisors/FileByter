using System;
using System.Reflection;

namespace FileByter
{
	public delegate string PropertyFormatter(object value);
	public delegate string HeaderFormatter(PropertyInfo propertyInfo);

	public class Property<T>
	{
		private readonly PropertyReader<T> _propertyReader;

		public Property(string propertyName, PropertyFormatter formatter, HeaderFormatter headerFormatter, int order = 0)
		{
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (formatter == null) throw new ArgumentNullException("formatter");
			if (headerFormatter == null) throw new ArgumentNullException("headerFormatter");

			PropertyName = propertyName;
			_propertyReader = new PropertyReader<T>(propertyName);
			HeaderFormatter = headerFormatter;
			ValueFormatter = formatter;
			Order = order;
		}

		public string PropertyName { get; private set; }
		public HeaderFormatter HeaderFormatter { get; set; }
		public PropertyFormatter ValueFormatter { get; set; }
		public int Order { get; set; }

		public string GetFormattedValue(T @object)
		{
			var originalValueOfProperty = _propertyReader.ReadValue(@object);
			return ValueFormatter(originalValueOfProperty);
		}

		public string GetFormattedHeader()
		{
			return HeaderFormatter(_propertyReader.PropertyInfo);
		}
	}
}
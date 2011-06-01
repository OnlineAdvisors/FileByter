using System;
using System.Reflection;

namespace FileByter
{
	public delegate string PropertyFormatter<T>(PropertyFormatterContext<T> propertyFormatterContext);
	public delegate string HeaderFormatter(PropertyInfo propertyInfo);

	public class PropertyFormatterContext<T>
	{
		private readonly T _row;
		private readonly object _readValue;

		public PropertyFormatterContext(T row, object readValue)
		{
			_row = row;
			_readValue = readValue;
		}

		public object ReadValue { get { return _readValue; } }

		public T Row { get { return _row; } }
	}

	public class Property<T>
	{
		private readonly PropertyReader<T> _propertyReader;

		public Property(string propertyName, PropertyFormatter<T> formatter, HeaderFormatter headerFormatter, int order = 0)
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
		public PropertyFormatter<T> ValueFormatter { get; set; }
		public int Order { get; set; }

		public string GetFormattedValue(T @object)
		{
			object originalValueOfProperty = _propertyReader.ReadValue(@object);
			var context = new PropertyFormatterContext<T>(@object, originalValueOfProperty);
			return ValueFormatter(context);
		}

		public string GetFormattedHeader()
		{
			return HeaderFormatter(_propertyReader.PropertyInfo);
		}
	}
}
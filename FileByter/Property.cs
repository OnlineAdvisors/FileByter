using System;
using System.Reflection;

namespace FileByter
{
	public delegate string PropertyFormatter(PropertyFormatterContext propertyFormatterContext);
	public class PropertyFormatterContext
	{
		protected readonly object RowItem;
		private readonly object _readValue;

		public PropertyFormatterContext(object row, object readValue)
		{
			RowItem = row;
			_readValue = readValue;
		}

		public object ReadValue { get { return _readValue; } }

		public object Row { get { return RowItem; } }
	}

	public delegate string PropertyFormatter<T>(PropertyFormatterContext<T> propertyFormatterContext);
	public delegate string HeaderFormatter(PropertyInfo propertyInfo);

	public class PropertyFormatterContext<T> : PropertyFormatterContext
	{
		public PropertyFormatterContext(T row, object readValue)
			: base(row, readValue)
		{
		}

		public new T Row { get { return (T)RowItem; } }
	}

	public class Property
	{
		
	}

	public class Property<T> : Property
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
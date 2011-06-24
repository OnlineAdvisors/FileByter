using System;
using System.Reflection;

namespace FileByter
{
	public delegate string PropertyFormatter(PropertyFormatterContext propertyFormatterContext);
	public delegate string HeaderFormatter(PropertyInfo propertyInfo);


	public class PropertyFormatterContext
	{
		protected readonly object RowObjectItem;
		private readonly object _itemValue;

		public PropertyFormatterContext(object rowObject, object itemValue)
		{
			RowObjectItem = rowObject;
			_itemValue = itemValue;
		}

		public object ItemValue { get { return _itemValue; } }

		public object RowObject { get { return RowObjectItem; } }
	}

	public class Property
	{
		private readonly PropertyReader _propertyReader;

		public Property(Type objectType, string propertyName, PropertyFormatter formatter, HeaderFormatter headerFormatter, int order = 0)
		{
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (formatter == null) throw new ArgumentNullException("formatter");
			if (headerFormatter == null) throw new ArgumentNullException("headerFormatter");

			PropertyName = propertyName;
			_propertyReader = new PropertyReader(objectType, propertyName);
			HeaderFormatter = headerFormatter;
			ValueFormatter = formatter;
			Order = order;
		}

		public string PropertyName { get; private set; }
		public HeaderFormatter HeaderFormatter { get; set; }
		public PropertyFormatter ValueFormatter { get; set; }
		public int Order { get; set; }

		public string GetFormattedValue(object @object)
		{
			object originalValueOfProperty = _propertyReader.ReadValue(@object);
			var context = new PropertyFormatterContext(@object, originalValueOfProperty);
			return ValueFormatter(context);
		}

		public string GetFormattedHeader()
		{
			return HeaderFormatter(_propertyReader.PropertyInfo);
		}
	}
}
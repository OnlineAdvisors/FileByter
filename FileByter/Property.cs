namespace FileByter
{
	public delegate string PropertyFormatter(object value);

	public class Property<T>
	{
		private readonly PropertyReader<T> _propertyReader;

		public Property(PropertyReader<T> propertyReader, PropertyFormatter formatter, int order)
		{
			_propertyReader = propertyReader;
			Formatter = formatter;
			Order = order;
		}

		public PropertyFormatter Formatter { get; set; }
		public int Order { get; set; }

		public string GetValue(T @object)
		{
			var originalValueOfProperty = _propertyReader.ReadValue(@object);
			return Formatter(originalValueOfProperty);
		}
	}
}
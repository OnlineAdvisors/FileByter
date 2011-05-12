namespace FileByter
{
	public delegate string PropertyFormatter(object value);

	public class Property<T>
	{
		private readonly PropertyReader<T> _propertyReader;

		public Property(PropertyReader<T> propertyReader, PropertyFormatter formatter)
		{
			_propertyReader = propertyReader;
			Formatter = formatter;
		}

		public PropertyFormatter Formatter { get; set; }

		public string GetValue(T @object)
		{
			var originalValueOfProperty = _propertyReader.ReadValue(@object);
			return Formatter(originalValueOfProperty);
		}
	}
}
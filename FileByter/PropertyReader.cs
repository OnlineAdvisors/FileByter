using System;
using System.Reflection;

namespace FileByter
{
	public class PropertyReader
	{
		public const int NullKeyHashCode = 0;

		private readonly Lazy<LateBoundProperty> _propertyValueReadProperty;
		private readonly PropertyInfo _propertyInfo;
		public PropertyInfo PropertyInfo { get { return _propertyInfo; } }

		public string PropertyName { get { return _propertyInfo.Name; } }

		public PropertyReader(Type objectType, string propertyName)
		{
			var typeOfT = objectType;

			_propertyInfo = typeOfT.GetProperty(propertyName);
			if (_propertyInfo == null)
				throw new ArgumentException("Could not find property name [{0}] on type [{1}]."
												.FormatWith(propertyName, typeOfT.FullName));

			_propertyValueReadProperty = new Lazy<LateBoundProperty>(() => DelegateFactory.Create(_propertyInfo));
		}

		public object ReadValue(object @rootObject)
		{
			return _propertyValueReadProperty.Value(@rootObject);
		}
	}
}
using System;
using System.Reflection;

namespace FileByter
{
	public class PropertyReader<T>
	{
		public const int NullKeyHashCode = 0;

		private LateBoundProperty _propertyValueReadProperty;
		private readonly PropertyInfo _propertyInfo;

		private LateBoundProperty PropertyValueReadProperty
		{
			get
			{
				// this property is "late-bound" because if this reader is never 
				// accessed, there's no need to expose the up front reflection cost
				return (_propertyValueReadProperty ??
				        (_propertyValueReadProperty = DelegateFactory.Create<T>(_propertyInfo)));
			}
		}

		public string PropertyName { get { return _propertyInfo.Name; } }

		public PropertyReader(string propertyName)
		{
			var typeOfT = typeof(T);

			_propertyInfo = typeOfT.GetProperty(propertyName);

			if (_propertyInfo == null)
				throw new ArgumentException("Could not find property name [{0}] on type [{1}]."
				                            	.FormatWith(propertyName, typeOfT.FullName));
		}

		public object ReadValue(T @rootObject)
		{
			return PropertyValueReadProperty(@rootObject);
		}

		/// <summary>
		/// Returns the class's configured Property's hashcode - or zero is the object is null
		/// </summary>
		public int GetItemHashCode(T @rootObject)
		{
			var value = ReadValue(@rootObject);

			if (value == null)
				return NullKeyHashCode;

			return value.GetHashCode();
		}
	}
}
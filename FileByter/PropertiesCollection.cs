﻿using System;
using System.Collections.Generic;

namespace FileByter
{
	public class PropertiesCollection<T>
	{
		private readonly HashSet<string> _excludedProperties = new HashSet<string>();

		private readonly Dictionary<string, Property<T>> _properties = new Dictionary<string, Property<T>>();

		public bool ContainsPropertyName(string propertyName)
		{
			return _properties.ContainsKey(propertyName);
		}

		private bool IsExcluded(string propertyName)
		{
			return _excludedProperties.Contains(propertyName);
		}

		public void AddExclusion(string propertyName)
		{
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			_excludedProperties.Add(propertyName);

			if (_properties.ContainsKey(propertyName))
				throw new InvalidOperationException(
					"Cannot add exclusion of property [{0}]. It has already been included explicitly.".FormatWith(propertyName));
		}

		public void AddProperty(string propertyName, Property<T> value)
		{
			if (IsExcluded(propertyName))
				throw new InvalidOperationException("Property [{0}] was already excluded. Cannot add an inclusion rule.".FormatWith(propertyName));

			_properties.Add(propertyName, value);
		}

		public Property<T> this[string propertyName]
		{
			get { return _properties[propertyName]; }
		}

		public IEnumerable<Property<T>> Values
		{
			get { return _properties.Values; }
		}

		public bool ContainsExcludedProperty(string propertyName)
		{
			return _excludedProperties.Contains(propertyName);
		}
	}
}
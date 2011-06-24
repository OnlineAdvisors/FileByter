using System;
using System.Linq.Expressions;

namespace FileByter
{
	public class TypeConfiguration
	{
		private readonly PropertiesCollection _properties = new PropertiesCollection();

		public PropertiesCollection Properties { get { return _properties; } }

		public void AddProperty(Property property)
		{
			var propertyName = property.PropertyName;

			// Should only add property once
			if (Properties.ContainsPropertyName(propertyName))
				throw new ArgumentException("The property [{0}] has been already been specified.".FormatWith(propertyName));

			Properties.AddProperty(propertyName, property);
		}
	}


	public class TypeConfiguration<T> : TypeConfiguration
	{
		public TypeConfiguration<T> Exclude<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			var propertyName = propertyExpression.GetMemberName();
			Properties.AddExclusion(propertyName);
			return this;
		}


		public TypeConfiguration<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter propertyFormatter, HeaderFormatter headerFormatter)
		{
			if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");
			if (propertyFormatter == null) throw new ArgumentNullException("propertyFormatter");

			var propertyName = propertyExpression.GetMemberName();

			if (headerFormatter == null)
				headerFormatter = pi => pi.Name;

			var property = new Property(typeof(T), propertyName, propertyFormatter, headerFormatter);

			Properties.AddProperty(propertyName, property);

			return this;
		}
		public TypeConfiguration<T> AddPropertyFormatter<TProperty>(Expression<Func<T, TProperty>> propertyExpression, PropertyFormatter formatter)
		{
			return AddPropertyFormatter(propertyExpression, formatter, null);
		}
	}
}
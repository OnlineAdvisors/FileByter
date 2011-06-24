using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FileByter
{
	public delegate string DelimeterFoundInValue(string propertyName, string columnDelimeter, string value);

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


	public class FileExportSpecification
	{
		protected readonly Dictionary<Type, TypeConfiguration> RowTypeConfigurations = new Dictionary<Type, TypeConfiguration>();

		private readonly HeaderFormatter _defaultHeaderFormatter = pi => pi.Name;
		public HeaderFormatter DefaultHeaderFormatter { get { return _defaultHeaderFormatter; } }
		public string ColumnDelimeter { get; set; }
		public string RowDelimeter { get; set; }
		public DelimeterFoundInValue OnDelimeterFoundInValue { get; set; }

		public FileExportSpecification(IEnumerable<Type> rowTypes, string columnDelimeter, string rowDelimeter)
		{
			if (rowTypes == null) throw new ArgumentNullException("rowTypes");

			var configType = typeof(TypeConfiguration<>);
			foreach (var rowType in rowTypes)
			{
				Type makeGenericType = configType.MakeGenericType(rowType);
				var instance = (TypeConfiguration)Activator.CreateInstance(makeGenericType);
				RowTypeConfigurations.Add(rowType, instance);
			}


			ColumnDelimeter = columnDelimeter;
			RowDelimeter = rowDelimeter;
			OnDelimeterFoundInValue = (string propertyName, string columnDelimeterX, string value) =>
			{
				throw new FileExportException
					("Item with propertyName[{0}] and value[{1}] contained column delimeter [{2}]"
						.FormatWith(propertyName, value, columnDelimeterX));
			};
		}

		public PropertiesCollection GetPropertiesForType<T>()
		{
			//TODO: dictionary.Contains & throw if type not configured.
			return RowTypeConfigurations[typeof(T)].Properties;
		}

		private TypeConfiguration GetTypeConfiguration<T>()
		{
			return RowTypeConfigurations[typeof(T)];
		}

		internal bool IsPropertyDefined<T>(string propertyName)
		{
			if (GetPropertiesForType<T>().ContainsExcludedProperty(propertyName))
				return true;
			return GetPropertiesForType<T>().ContainsPropertyName(propertyName);
		}

		public bool IsPropertyExcluded<T>(string propertyName)
		{
			return GetPropertiesForType<T>().IsExcluded(propertyName);
		}

		private bool _excludeNonConfiguredProperties;
		internal string PrePendFileWithValue { get; private set; }
		internal string AppendFileWithValue { get; private set; }
		internal bool SkipNonConfiguredProperties { get { return _excludeNonConfiguredProperties; } }
		public void ExcludeNonConfiguredProperties()
		{
			_excludeNonConfiguredProperties = true;

		}

		public bool IncludeHeader { get; set; }

		public void PrependFileWith(string value)
		{
			PrePendFileWithValue = value;
		}

		public void AppendFileWith(string value)
		{
			AppendFileWithValue = value;
		}

		public void ConfigureType<T>(Action<TypeConfiguration<T>> action)
		{
			var typeConfiguration = GetTypeConfiguration<T>();
			action((TypeConfiguration<T>)typeConfiguration);
		}
	}
}
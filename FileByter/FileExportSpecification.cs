using System;
using System.Collections;
using System.Collections.Generic;

namespace FileByter
{
	public delegate string DelimeterFoundInValue(string propertyName, string columnDelimeter, string value);

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
			return GetPropertiesForType(typeof(T));
		}

		public PropertiesCollection GetPropertiesForType(Type type)
		{
			if(RowTypeConfigurations.ContainsKey(type))
				return RowTypeConfigurations[type].Properties;

			var msg = string.Join(", ", RowTypeConfigurations.Values);

			throw new FileExportException("Cannot export unknown type [{0}] as it was not defined within the file export specification. The following types have been defined [{1}]".FormatWith(type.FullName, msg));
		}

		internal TypeConfiguration GetTypeConfiguration<T>()
		{
			return GetTypeConfiguration(typeof(T));
		}

		internal TypeConfiguration GetTypeConfiguration(Type type)
		{
			return RowTypeConfigurations[type];
		}

		private bool _excludeNonConfiguredProperties;
		private readonly IDictionary<Type, PropertyFormatter> _globalDefaultFormatters = new Dictionary<Type, PropertyFormatter>();
		internal string PrePendFileWithValue { get; private set; }
		internal string AppendFileWithValue { get; private set; }
		internal bool SkipNonConfiguredProperties { get { return _excludeNonConfiguredProperties; } }
		public void ExcludeNonConfiguredProperties()
		{
			_excludeNonConfiguredProperties = true;
		}

		public bool IncludeHeader { get; set; }

		public IEnumerable<TypeConfiguration> ConfiguredTypes
		{
			get { return RowTypeConfigurations.Values; }
		}

		public IDictionary<Type, PropertyFormatter> DefaultPropertyFormatters
		{
			get { return _globalDefaultFormatters; }
		}

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

		public void AddDefault<TProperty>(Func<TProperty, string> formatter)
		{
			PropertyFormatter pf = context => formatter((TProperty)context.ItemValue);
			//DefaultTypeFormatters.Add(inputType, pf);

			_globalDefaultFormatters.Add(typeof(TProperty), pf);
		}

	}
}
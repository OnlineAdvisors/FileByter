using Xunit;

namespace FileByter.Tests
{
	public class FileExportSpecificationTests : SpecBase
	{
		private readonly FileExport<SimpleObject> _specFactory = new FileExport<SimpleObject>();

		[Fact]
		public void Should_use_the_custom_formatter()
		{
			var fileExportSpecification = _specFactory.CreateSpec(cfg =>
			{
				cfg.ConfigureType<SimpleObject>(typeCfg =>
				{
					typeCfg.AddPropertyFormatter(x => x.Id, context => context.ItemValue.ToString() + "_TEST"); ;
				}); ;
			});

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification.GetPropertiesForType<SimpleObject>()["Id"].GetFormattedValue(simpleObject).ShouldEqual("2_TEST");
		}

		[Fact]
		public void Should_use_the_default_formatter_of_object()
		{
			var fileExportSpecification = _specFactory.CreateSpec();
			fileExportSpecification.ColumnDelimeter = "	";

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification.GetPropertiesForType<SimpleObject>()["Id"].GetFormattedValue(simpleObject).ShouldEqual("2");
		}

		[Fact]
		public void Should_use_a_specially_configured_type_default()
		{
			var exportSpecification = new FileExport<SimpleObject>();
			exportSpecification.AddDefault<int>(value => value + "_ASDF");

			var fileExportSpecification = exportSpecification.CreateSpec();

			var simpleObject = new SimpleObject { Id = 2, StringValue1 = "HELLO" };

			// Should use the "globally" configured formatter
			fileExportSpecification.GetPropertiesForType<SimpleObject>()["Id"].GetFormattedValue(simpleObject).ShouldEqual("2_ASDF");

			// Should not do any special formatting
			fileExportSpecification.GetPropertiesForType<SimpleObject>()["StringValue1"].GetFormattedValue(simpleObject).ShouldEqual("HELLO");
		}

		[Fact]
		public void Shoule_be_able_to_save_items_to_file()
		{
			var items = new[]
				            	{
				            		new SimpleObject {Id = 1, StringValue1 = "HELLO"},
				            		new SimpleObject {Id = 2, StringValue1 = "WORLD"},
				            	};

			var actual = GetExportResult(items, cfg =>
													{
													});

			actual.ShouldEqual(@"1,HELLO
2,WORLD
");
		}

		[Fact]
		public void Shoule_be_able_to_exclude_a_property()
		{
			var items = new[]
			{
				new SimpleObject {Id = 1, StringValue1 = "HELLO"},
				new SimpleObject {Id = 2, StringValue1 = "WORLD"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.ConfigureType<SimpleObject>(typeCfg =>
				{
					typeCfg.Exclude(x => x.StringValue1); ;
				}); ;
			});

			actual.ShouldEqual(@"1
2
");
		}


		public class SimpleObject
		{
			public int Id { get; set; }
			public string StringValue1 { get; set; }
		}

		public class SimpleObjectWithNullable
		{
			public int? Id { get; set; }
			public string StringValue1 { get; set; }
		}

		[Fact]
		public void Should_output_empty_items_with_delimeter_correctly()
		{
			var items = new[]
				            	{
				            		new SimpleObjectWithNullable {Id = null, StringValue1 = "HELLO"},
				            		new SimpleObjectWithNullable {Id = 2, StringValue1 = "WORLD"},
				            	};

			var actual = GetExportResult(items, cfg => { });

			actual.ShouldEqual(@",HELLO
2,WORLD
");
		}


		[Fact]
		public void Should_be_able_to_exclude_all_non_specified_properties()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = null, StringValue1 = "HELLO"},
				new SimpleObjectWithNullable {Id = 2, StringValue1 = "WORLD"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.ConfigureType<SimpleObjectWithNullable>(typeCfg =>
				{
					typeCfg.AddPropertyFormatter(p => p.StringValue1, context => context.ItemValue.ToString()); ;
				}); ;
				cfg.ExcludeNonConfiguredProperties();
			});

			actual.ShouldEqual(@"HELLO
WORLD
");
		}

		[Fact]
		public void Should_be_able_to_apply_row_level_conditional_logic()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = null, StringValue1 = "HELLO"},
				new SimpleObjectWithNullable {Id = 2, StringValue1 = "WORLD"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.ConfigureType<SimpleObjectWithNullable>(typeCfg =>
				{
					typeCfg.AddPropertyFormatter(p => p.StringValue1, (context) => context.RowObject.Cast<SimpleObjectWithNullable>().Id.HasValue ? "NOTEMPTYID" : "EMPTYID"); ;
				}); ;
				cfg.ExcludeNonConfiguredProperties();
			});

			actual.ShouldEqual(@"EMPTYID
NOTEMPTYID
");
		}


		[Fact]
		public void Should_be_able_to_prepend_a_value_to_the_file()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = 1, StringValue1 = "HELLO"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.PrependFileWith("SaySomething");
			});

			actual.ShouldEqual(@"SaySomething
1,HELLO
");
		}


		[Fact]
		public void Should_be_able_to_append_a_value_to_the_file()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = 1, StringValue1 = "HELLO"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.AppendFileWith("SaySomething");
			});

			actual.ShouldEqual(@"1,HELLO
SaySomething");
		}


		[Fact]
		public void When_a_value_contains_a_delimeter_then_should_throw_an_exception()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = 1, StringValue1 = "HELLO~There"},
			};

			typeof(FileExportException).ShouldBeThrownBy(() =>
			{
				var actual = GetExportResult(items, cfg =>
				{
					cfg.ColumnDelimeter = "~";
				});
			});
		}


		[Fact]
		public void When_a_value_contains_a_delimeter_is_found_should_be_able_to_provide_handler()
		{
			var items = new[]
			{
				new SimpleObjectWithNullable {Id = 1, StringValue1 = "HELLO~There"},
			};

			var actual = GetExportResult(items, cfg =>
			{
				cfg.ColumnDelimeter = "~";
				cfg.OnDelimeterFoundInValue = (propertyName, delimeter, value) => "SomeOtherValue";
			});

			actual.ShouldEqual(@"1~SomeOtherValue
");

		}


		[Fact(Skip = "TODO")]
		public void Should_be_able_to_export_rows_of_different_types()
		{
			var items = new object[]
			{
				new SimpleObjectWithNullable {Id = 1, StringValue1 = "HELLO There"},
				new SimpleObject{Id=2,StringValue1 = "What?"}
			};

			var actual = GetExportResult(items, cfg =>
			{
			});

			actual.ShouldEqual(@"1,HELLO There
2,What?
");
		}
	}

	public static class Extensions
	{
		public static T Cast<T>(this object itemToCast)
		{
			return (T)itemToCast;
		}
	}
}
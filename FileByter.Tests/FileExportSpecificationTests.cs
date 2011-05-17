using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FileByter.Tests
{
	public class FileExportSpecificationTests : SpecBase
	{
		private readonly FileExport<SimpleObject> _specFactory = new FileExport<SimpleObject>();

		[Fact]
		public void Should_use_the_custom_formatter()
		{
			var fileExportSpecification = _specFactory.Create(cfg =>
			{
				cfg.AddPropertyFormatter(x => x.Id, v => v.ToString() + "_TEST"); ;
			});
		[Fact]
		public void Should_use_the_custom_formatter()
		{
			var fileExportSpecification = _specFactory.CreateSpec(cfg =>
			{
				cfg.AddPropertyFormatter(x => x.Id, v => v.ToString() + "_TEST"); ;
			});

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification["Id"].GetFormattedValue(simpleObject).ShouldEqual("2_TEST");
		}

		[Fact]
		public void Should_use_the_default_formatter_of_object()
		{
			var fileExportSpecification = _specFactory.Create();
			fileExportSpecification.ColumnDelimeter = "	";
		[Fact]
		public void Should_use_the_default_formatter_of_object()
		{
			var fileExportSpecification = _specFactory.CreateSpec();
			fileExportSpecification.ColumnDelimeter = "	";

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification["Id"].GetFormattedValue(simpleObject).ShouldEqual("2");
		}

		[Fact]
		public void Should_use_a_specially_configured_type_default()
		{
			var exportSpecification = new FileExport<SimpleObject>();
			exportSpecification.AddDefault<int>(value => value + "_ASDF");

			var fileExportSpecification = exportSpecification.CreateSpec();

			var simpleObject = new SimpleObject { Id = 2, StringValue1 = "HELLO" };

			// Should use the "globally" configured formatter
			fileExportSpecification["Id"].GetFormattedValue(simpleObject).ShouldEqual("2_ASDF");

			// Should not do any special formatting
			fileExportSpecification["StringValue1"].GetFormattedValue(simpleObject).ShouldEqual("HELLO");
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
2,WORLD");
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
				cfg.Exclude(x => x.StringValue1); ;
			});

			actual.ShouldEqual(@"1
2");

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
2,WORLD");
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
														cfg.AddPropertyFormatter(p => p.StringValue1, p => p.ToString());
														cfg.ExcludeNonConfiguredProperties();
													});

			actual.ShouldEqual(@"HELLO
WORLD");
		}
	public class FileExportPropertyOrderingTests
	{
		public class TestOrderingObject
		{
			public TestOrderingObject()
			{
				Property1 = 1;
				Property2 = 2;
				Property3 = 3;
				Property4 = 4;
				Property5 = 5;
				Property6 = 6;
			}

			public int Property1 { get; set; }
			public int Property2 { get; set; }
			public int Property3 { get; set; }
			public int Property4 { get; set; }
			public int Property5 { get; set; }
			public int Property6 { get; set; }
		}

		[Fact]
		public void Default_configuration_should_place_properties_in_correct_order()
		{
			var items = new[] { new TestOrderingObject() };

			var actual = GetExportResult(items, cfg => { /*no overriding config - use defaults*/});

			actual.ShouldEqual(@"1,2,3,4,5,6");
		}


		[Fact]
		public void Custom_property_should_still_show_up_in_correct_order()
		{
			var items = new[] { new TestOrderingObject() };

			var actual = GetExportResult(items, cfg =>
			{
				cfg.AddPropertyFormatter(p => p.Property3, p => p.ToString()); ;
			});

			actual.ShouldEqual(@"1,2,3,4,5,6");
		}

		[Fact]
		public void Custom_property_with_exclusion_should_still_show_up_in_correct_order()
		{
			var items = new[] { new TestOrderingObject() };

			var actual = GetExportResult(items, cfg =>
			{
				cfg.AddPropertyFormatter(p => p.Property3, p => p.ToString()); ;
				cfg.Exclude(p => p.Property2);
			});

			actual.ShouldEqual(@"1,3,4,5,6");
		}


		private static string GetExportResult<T>(IEnumerable<T> items, Action<FileExportSpecification<T>> config)
		{
			var factory = new FileExportSpecificationFactory<T>();
			var fileExportSpecification = factory.CreateSpec(config);
			var fileExporter = factory.CreateFileExporter(fileExportSpecification);

			return fileExporter.ExportToString(items);
		}
	}

}

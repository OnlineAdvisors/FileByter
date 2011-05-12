using System.Collections.Generic;
using System.IO;
using Xunit;

namespace FileByter.Tests
{
	public class FileExportSpecificationTests
	{
		private readonly FileExportSpecificationFactory<SimpleObject> _specFactory = new FileExportSpecificationFactory<SimpleObject>();

		[Fact]
		public void Should_use_the_custom_formatter()
		{
			var fileExportSpecification = _specFactory.Create(cfg =>
			{
				cfg.AddPropertyFormatter(x => x.Id, v => v.ToString() + "_TEST"); ;
			});

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification["Id"].GetValue(simpleObject).ShouldEqual("2_TEST");
		}

		[Fact]
		public void Should_use_the_default_formatter_of_object()
		{
			var fileExportSpecification = _specFactory.Create();
			fileExportSpecification.ColumnDelimeter = "	";

			var simpleObject = new SimpleObject { Id = 2 };

			fileExportSpecification["Id"].GetValue(simpleObject).ShouldEqual("2");
		}

		[Fact]
		public void Should_use_a_specially_configured_type_default()
		{
			var exportSpecification = new FileExportSpecificationFactory<SimpleObject>();
			exportSpecification.AddDefault<int>(value => value + "_ASDF");

			var fileExportSpecification = exportSpecification.Create();

			var simpleObject = new SimpleObject { Id = 2, StringValue1 = "HELLO" };

			// Should use the "globally" configured formatter
			fileExportSpecification["Id"].GetValue(simpleObject).ShouldEqual("2_ASDF");

			// Should not do any special formatting
			fileExportSpecification["StringValue1"].GetValue(simpleObject).ShouldEqual("HELLO");
		}

		[Fact]
		public void Shoule_be_able_to_save_items_to_file()
		{
			var simpleObject = new List<SimpleObject>
			{
				new SimpleObject {Id = 1, StringValue1 = "HELLO"},
				new SimpleObject {Id = 2, StringValue1 = "WORLD"},
			};

			string filePath = Path.GetTempFileName();

			var spec = _specFactory.Create();
			_specFactory.CreateFileExporter(spec).ExportToFile(simpleObject, filePath);

			const string expected = @"1,HELLO
2,WORLD";
			var actual = File.ReadAllText(filePath);

			actual.ShouldEqual(expected);
		}

		[Fact]
		public void Shoule_be_able_to_exclude_a_property()
		{
			var simpleObject = new List<SimpleObject>
			{
				new SimpleObject {Id = 1, StringValue1 = "HELLO"},
				new SimpleObject {Id = 2, StringValue1 = "WORLD"},
			};

			string filePath = Path.GetTempFileName();

			var fileExportSpecification = new FileExportSpecificationFactory<SimpleObject>();
			var spec = fileExportSpecification.Create(cfg =>
			{
				cfg.Exclude(x => x.StringValue1); ;
			});

			_specFactory.CreateFileExporter(spec).ExportToFile(simpleObject, filePath);

			const string expected = @"1
2";
			var actual = File.ReadAllText(filePath);

			actual.ShouldEqual(expected);
		}
		public class SimpleObject
		{
			public int Id { get; set; }
			public string StringValue1 { get; set; }
		}
	}

}
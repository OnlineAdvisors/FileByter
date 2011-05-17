using Xunit;

namespace FileByter.Tests
{
	public class FileExportHeaderTests : SpecBase
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
		public void Default_header_should_use_propertyName()
		{
			var items = new[] { new TestOrderingObject() };

			var actual = GetExportResult(items, cfg =>
			{

			});

			actual.ShouldEqual(@"1,2,3,4,5,6");
		}
	}
}
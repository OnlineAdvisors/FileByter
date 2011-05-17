using Xunit;

namespace FileByter.Tests
{
	public class FileExportPropertyOrderingTests : SpecBase
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

	}
}
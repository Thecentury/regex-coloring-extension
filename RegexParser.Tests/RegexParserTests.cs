using NUnit.Framework;

namespace RegexParsing.Tests
{
	[TestFixture]
	public sealed class RegexParserTests
	{
		[TestCase( "abc" )]
		public void ShouldParse( string regex )
		{
			var tokens = RegexParser.ParseRegex( regex );
		}
	}
}

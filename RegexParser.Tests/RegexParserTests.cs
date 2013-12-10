using System;
using NUnit.Framework;

namespace RegexParsing.Tests
{
	[TestFixture]
	public sealed class RegexParserTests
	{
		[TestCase( "abc" )]
		[TestCase( "[a-b]" )]
		[TestCase( "[a-b23]" )]
		[TestCase( "[a-b23-5]" )]
		[TestCase( "[^a-b23-5]" )]
		public void ShouldParse( string regex )
		{
			var tokens = RegexParser.ParseRegex( regex );

			string str = String.Join( ",", tokens );
			Console.WriteLine( str );
		}

		[TestCase( "[^1-2]", true )]
		[TestCase( "[1-2]", false )]
		public void ShouldParseExcludeList( string regex, bool expectedExclude )
		{
			var tokens = RegexParser.ParseRegex( regex );

			CharList firstToken = (CharList)tokens[0];

			Assert.That( firstToken.Exclude, Is.EqualTo( expectedExclude ) );
		}
	}
}

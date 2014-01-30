using System;
using System.Linq;
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
		[TestCase( "(a)" )]
		[TestCase( "(a[a-c])" )]
		[TestCase( "(?:)" )]
		[TestCase( "<(a|b)" )]
		[TestCase( "(a)(b)" )]
		[TestCase( "(?<a>)" )]
		[TestCase( @"(""?)(b)" )]
		[TestCase( "<(img|td|table|body)[^>]+?(src|background)=(\"?)(?<path>[^ \">]+).*?>" )]
		public void ShouldParse( string regex )
		{
			var tokens = RegexParser.ParseRegex( regex );

			Assert.That( tokens, Has.Count.GreaterThan( 0 ) );

			string str = String.Join( ",", tokens );
			Console.WriteLine( str );
		}

		[TestCase( "a+", 1, null )]
		[TestCase( "a?", 0, 1 )]
		[TestCase( "a*", 0, null )]
		[TestCase( "a{,2}", 0, 2 )]
		[TestCase( "a{1,2}", 1, 2 )]
		[TestCase( "a{1,}", 1, null )]
		public void Quantifiers( string regex, int expectedMin, int? expectedMax )
		{
			var tokens = RegexParser.ParseRegex( regex );

			var quantifier = tokens.OfType<Quantifier>().First();

			Assert.That( quantifier.MinAmount, Is.EqualTo( expectedMin ) );
			Assert.That( quantifier.MaxAmount, Is.EqualTo( expectedMax ) );
		}

		[TestCase( "[^1-2]", true )]
		[TestCase( "[1-2]", false )]
		public void ShouldParseExcludeList( string regex, bool expectedExclude )
		{
			var tokens = RegexParser.ParseRegex( regex );

			Assert.That( tokens, Has.Count.GreaterThan( 0 ) );

			CharList firstToken = (CharList)tokens[0];

			Assert.That( firstToken.Exclude, Is.EqualTo( expectedExclude ) );
		}
	}
}

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
		public void ShouldParse( string regex )
		{
			var tokens = RegexParser.ParseRegex( regex );

			string str = String.Join( ",", tokens );
			Console.WriteLine( str );
		}
	}
}

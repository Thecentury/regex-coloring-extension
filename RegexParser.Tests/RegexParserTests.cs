﻿using System;
using NUnit.Framework;

namespace RegexParsing.Tests
{
	[TestFixture]
	public sealed class RegexParserTests
	{
		[TestCase( "abc" )]
		[TestCase( "[a-b]" )]
		public void ShouldParse( string regex )
		{
			var tokens = RegexParser.ParseRegex( regex );

			string str = String.Join( ",", tokens );
			Console.WriteLine( str );
		}
	}
}
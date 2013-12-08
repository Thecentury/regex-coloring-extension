using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sprache;

namespace RegexParsing
{
	public static class RegexParser
	{
		public static List<RegexToken> ParseRegex( string regex )
		{
			if ( regex == null )
			{
				throw new ArgumentNullException( "regex" );
			}

			return Parser.Parse( regex );
		}

		private static readonly Parser<VerbatimString> Verbatim =
			Parse.AnyChar.AtLeastOnce().Text().Select( s => new VerbatimString { Value = s } );

		private static readonly Parser<List<RegexToken>> Parser = Verbatim.Many().Select( e => e.Cast<RegexToken>().ToList() ).End();
	}

	public abstract class RegexToken
	{

	}

	public sealed class VerbatimString : RegexToken
	{
		public string Value { get; set; }
	}
}

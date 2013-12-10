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

		private static readonly Parser<RegexToken> Verbatim =
			Parse.AnyChar.AtLeastOnce().Text().Select( s => new VerbatimString { Value = s } );

		private static readonly Parser<ListItem> Range =
			from start in Parse.AnyChar
			from delimiter in Parse.Char( '-' )
			from end in Parse.AnyChar
			select new CharRange { Start = start, End = end };

		private static readonly Parser<RegexToken> CharList =
			from open in Parse.Char( '[' )
			from exclude in Parse.Char( '^' ).Optional()
			from inner in Range.Or( Parse.Char( c => c != ']', "not ]" ).Select( c => new SingleChar { Value = c } ) ).AtLeastOnce()
			from close in Parse.Char( ']' )
			select new CharList { Items = inner.ToList(), Exclude = exclude.IsDefined };

		private static readonly Parser<List<RegexToken>> Parser = CharList.Or( Verbatim ).Many().Select( e => e.ToList() ).End();
	}

	public abstract class RegexToken
	{

	}

	public abstract class ListItem
	{
	}

	public sealed class CharList : RegexToken
	{
		public bool Exclude { get; set; }

		public List<ListItem> Items { get; set; }

		public override string ToString()
		{
			return String.Format( "[{0}]", String.Join( "", Items ) );
		}
	}

	public sealed class SingleChar : ListItem
	{
		public char Value { get; set; }

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public sealed class CharRange : ListItem
	{
		public char Start { get; set; }

		public char End { get; set; }

		public override string ToString()
		{
			return String.Format( "Range {0}-{1}", Start, End );
		}
	}

	public sealed class VerbatimString : RegexToken
	{
		public string Value { get; set; }

		public override string ToString()
		{
			return String.Format( "\"{0}\"", Value );
		}
	}
}

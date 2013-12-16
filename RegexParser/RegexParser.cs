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
			Parse.AnyChar.AtLeastOnce().Text().Select( s => new VerbatimString { Value = s } ).Positioned();

		private static readonly Parser<ListItem> Range =
			( from start in Parse.AnyChar
			  from delimiter in Parse.Char( '-' )
			  from end in Parse.AnyChar
			  select new CharRange { Start = start, End = end } ).Positioned();

		private static readonly Parser<RegexToken> CharList =
			( from open in Parse.Char( '[' )
			  from exclude in Parse.Char( '^' ).Optional()
			  from inner in Range.Or( Parse.Char( c => c != ']', "not ]" ).Select( c => new SingleChar { Value = c } ) ).Positioned().AtLeastOnce()
			  from close in Parse.Char( ']' )
			  select new CharList { Items = inner.ToList(), Exclude = exclude.IsDefined } ).Positioned();

		private static readonly Parser<List<RegexToken>> Parser = CharList.Or( Verbatim ).Many().Select( e => e.ToList() ).End();
	}

	public abstract class RegexToken : IPositionAware
	{
		private Position _position;
		private int _length;

		public Position Position
		{
			get { return _position; }
		}

		public int Length
		{
			get { return _length; }
		}

		public void SetPos( Position startPos, int length )
		{
			_position = startPos;
			_length = length;
		}
	}

	public abstract class ListItem : IPositionAware
	{
		private Position _position;
		private int _length;

		public void SetPos( Position startPos, int length )
		{
			_position = startPos;
			_length = length;
		}
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

	public class Quantifier : RegexToken
	{
		public int MinAmount { get; set; }

		public int MaxAmount { get; set; }

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

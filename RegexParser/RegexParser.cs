using System;
using System.Collections.Generic;
using System.Linq;
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

			return _parser.Parse( regex );
		}

		public static IResult<List<RegexToken>> TryParseRegex( string regex )
		{
			if ( regex == null )
			{
				throw new ArgumentNullException( "regex" );
			}

			return _parser.TryParse( regex );
		}

		// http://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx
		private static readonly char[] _charsToEscape = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

		private static readonly Parser<RegexToken> _verbatim =
			Parse.Char( c => !_charsToEscape.Contains( c ), "Common characters" ).AtLeastOnce().Text().Select( s => new VerbatimString { Value = s } ).Positioned();

		private static readonly Parser<ListItem> _range =
			( from start in Parse.AnyChar
			  from delimiter in Parse.Char( '-' )
			  from end in Parse.AnyChar
			  select new CharRange { Start = start, End = end } ).Positioned();

		private static readonly Parser<int> _integer = Parse.Number.Select( d => Int32.Parse( d ) );

		private static readonly Parser<RegexToken> _quantifier =
			Parse.Char( '*' ).Return( new Quantifier { MinAmount = 0 } )
				.Or( Parse.Char( '+' ).Return( new Quantifier { MinAmount = 1 } ) )
				.Or( Parse.Char( '?' ).Return( new Quantifier { MinAmount = 0, MaxAmount = 1 } ) )
				.Or(
					from openBracket in Parse.Char( '{' )
					from min in _integer.Optional()
					from comma in Parse.Char( ',' )
					from max in _integer.Optional()
					from closeBracket in Parse.Char( '}' )
					select new Quantifier { MinAmount = min.GetOrDefault(), MaxAmount = max.AsNullable() }
				).Positioned();

		private static readonly Parser<RegexToken> _charList =
			( from open in Parse.Char( '[' )
			  from exclude in Parse.Char( '^' ).Optional()
			  from inner in _range.Or( Parse.Char( c => c != ']', "not ]" ).Select( c => new SingleChar { Value = c } ) ).Positioned().AtLeastOnce()
			  from close in Parse.Char( ']' )
			  select new CharList { Items = inner.ToList(), Exclude = exclude.IsDefined } ).Positioned();

		private static readonly Parser<RegexToken> _quantifiable = _charList.Or( _verbatim );

		private static IEnumerable<T> AsEnumerable<T>( T t1, IOption<T> t2 )
		{
			yield return t1;

			if ( t2.IsDefined )
			{
				yield return t2.Get();
			}
		}

		private static readonly Parser<IEnumerable<RegexToken>> _quantifiableWithOptionalQuantifier =
			from obj in _quantifiable
			from q in _quantifier.Optional()
			select AsEnumerable( obj, q );

		private static readonly Parser<IEnumerable<RegexToken>> _quantified =
			_quantifiableWithOptionalQuantifier.Many().Select( ts => ts.SelectMany( t => t ) );

		private static readonly Parser<List<RegexToken>> _parser = _quantified.Select( e => e.ToList() ).End();
	}

	public static class OptionExtensions
	{
		public static T? AsNullable<T>( this IOption<T> option ) where T : struct
		{
			return option.IsEmpty ? new T?() : option.Get();
		}
	}

	public enum PrimitiveRegexTokenKind
	{
		OpenSquareBracket,

		CharacterListNegation,

		CloseSquareBracket,

		RangeStart,

		RangeSymbol,

		RangeEnd,

		RepetitionsCount,

		LiteralCharacter,

		LiteralString
	}

	public sealed class PrimitiveRegexToken
	{
		private readonly PrimitiveRegexTokenKind _kind;
		private readonly int _start;
		private readonly int _length;

		public PrimitiveRegexToken( PrimitiveRegexTokenKind kind, int start, int length )
		{
			_kind = kind;
			_start = start;
			_length = length;
		}

		public PrimitiveRegexTokenKind Kind
		{
			get { return _kind; }
		}

		public int Start
		{
			get { return _start; }
		}

		public int Length
		{
			get { return _length; }
		}
	}

	public static class ImperativePositionAwareExtensions
	{
		public static PrimitiveRegexToken ToPrimitiveToken( this IImperativePositionAware positionAware,
			PrimitiveRegexTokenKind kind )
		{
			return new PrimitiveRegexToken( kind, positionAware.Position.Pos, positionAware.Length );
		}
	}

	public abstract class RegexToken : IImperativePositionAware
	{
		private Position _position;
		private int _length;

		public Position Position
		{
			get { return _position; }
		}

		public int Offset
		{
			get { return _position.Pos; }
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

		public abstract IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens();
	}

	public abstract class ListItem : IImperativePositionAware
	{
		private Position _position;
		private int _length;

		public void SetPos( Position startPos, int length )
		{
			_position = startPos;
			_length = length;
		}

		public Position Position
		{
			get { return _position; }
		}

		public int Length
		{
			get { return _length; }
		}

		public abstract IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens();
	}

	public sealed class CharList : RegexToken
	{
		public bool Exclude { get; set; }

		public List<ListItem> Items { get; set; }

		public override string ToString()
		{
			return String.Format( "[{0}]", String.Join( "", Items ) );
		}

		public override IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens()
		{
			yield return new PrimitiveRegexToken( PrimitiveRegexTokenKind.OpenSquareBracket, Offset, 1 );

			foreach ( var item in Items )
			{
				foreach ( var token in item.GetPrimitiveTokens() )
				{
					yield return token;
				}
			}

			yield return new PrimitiveRegexToken( PrimitiveRegexTokenKind.CloseSquareBracket, Offset + Length - 1, 1 );
		}
	}

	public class Quantifier : RegexToken
	{
		public int MinAmount { get; set; }

		public int? MaxAmount { get; set; }

		public override IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens()
		{
			yield return this.ToPrimitiveToken( PrimitiveRegexTokenKind.RepetitionsCount );
		}
	}

	public sealed class SingleChar : ListItem
	{
		public char Value { get; set; }

		public override string ToString()
		{
			return Value.ToString();
		}

		public override IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens()
		{
			yield return this.ToPrimitiveToken( PrimitiveRegexTokenKind.LiteralCharacter );
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

		public override IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens()
		{
			int start = Position.Pos;

			yield return new PrimitiveRegexToken( PrimitiveRegexTokenKind.RangeStart, start, 1 );
			yield return new PrimitiveRegexToken( PrimitiveRegexTokenKind.RangeSymbol, start + 1, 1 );
			yield return new PrimitiveRegexToken( PrimitiveRegexTokenKind.RangeEnd, start + 2, 1 );
		}
	}

	public sealed class VerbatimString : RegexToken
	{
		public string Value { get; set; }

		public override string ToString()
		{
			return String.Format( "\"{0}\"", Value );
		}

		public override IEnumerable<PrimitiveRegexToken> GetPrimitiveTokens()
		{
			yield return this.ToPrimitiveToken( PrimitiveRegexTokenKind.LiteralString );
		}
	}
}

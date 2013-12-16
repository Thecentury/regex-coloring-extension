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

		// http://msdn.microsoft.com/en-us/library/4edbef7e(v=vs.110).aspx
		private static readonly char[] _charsToEscape = { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\' };

		private static readonly Parser<RegexToken> Verbatim =
			Parse.Char( c => !_charsToEscape.Contains( c ), "Common characters" ).AtLeastOnce().Text().Select( s => new VerbatimString { Value = s } ).Positioned();

		private static readonly Parser<ListItem> Range =
			( from start in Parse.AnyChar
			  from delimiter in Parse.Char( '-' )
			  from end in Parse.AnyChar
			  select new CharRange { Start = start, End = end } ).Positioned();

		private static readonly Parser<int> Integer = Parse.Number.Select( d => Int32.Parse( d ) );

		private static readonly Parser<RegexToken> Quantifier =
			Parse.Char( '*' ).Return( new Quantifier { MinAmount = 0 } )
				.Or( Parse.Char( '+' ).Return( new Quantifier { MinAmount = 1 } ) )
				.Or( Parse.Char( '?' ).Return( new Quantifier { MinAmount = 0, MaxAmount = 1 } ) )
				.Or(
					from openBracket in Parse.Char( '{' )
					from min in Integer.Optional()
					from comma in Parse.Char( ',' )
					from max in Integer.Optional()
					from closeBracket in Parse.Char( '}' )
					select new Quantifier { MinAmount = min.GetOrDefault(), MaxAmount = max.AsNullable() }
				).Positioned();

		private static readonly Parser<RegexToken> CharList =
			( from open in Parse.Char( '[' )
			  from exclude in Parse.Char( '^' ).Optional()
			  from inner in Range.Or( Parse.Char( c => c != ']', "not ]" ).Select( c => new SingleChar { Value = c } ) ).Positioned().AtLeastOnce()
			  from close in Parse.Char( ']' )
			  select new CharList { Items = inner.ToList(), Exclude = exclude.IsDefined } ).Positioned();

		private static readonly Parser<RegexToken> Quantifiable = CharList.Or( Verbatim );

		private static IEnumerable<T> AsEnumerable<T>( T t1, IOption<T> t2 )
		{
			yield return t1;

			if ( t2.IsDefined )
			{
				yield return t2.Get();
			}
		}

		private static readonly Parser<IEnumerable<RegexToken>> QuantifiableWithOptionalQuantifier =
			from obj in Quantifiable
			from q in Quantifier.Optional()
			select AsEnumerable( obj, q );

		private static readonly Parser<IEnumerable<RegexToken>> Quantified =
			QuantifiableWithOptionalQuantifier.Many().Select( ts => ts.SelectMany( t => t ) );

		private static readonly Parser<List<RegexToken>> Parser = Quantified.Select( e => e.ToList() ).End();
	}

	public static class ParserExtensions
	{
		public static Parser<TCasted> Cast<T, TCasted>( this Parser<T> parser ) where T : TCasted
		{
			return i =>
			{
				IResult<T> result = parser( i );

				if ( result.WasSuccessful )
				{
					return Result.Success<TCasted>( result.Value, result.Remainder );
				}
				else
				{
					return Result.Failure<TCasted>( result.Remainder, result.Message, result.Expectations );
				}
			};
		}
	}

	public static class OptionExtensions
	{
		public static T? AsNullable<T>( this IOption<T> option ) where T : struct
		{
			return option.IsEmpty ? new T?() : option.Get();
		}
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

		public int? MaxAmount { get; set; }
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

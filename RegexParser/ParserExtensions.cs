using Sprache;

namespace RegexParsing
{
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

		/// <summary>
		/// Construct a parser that will set the position to the position-aware
		/// T on succsessful match.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static Parser<T> Positioned<T>( this Parser<T> parser ) where T : IImperativePositionAware
		{
			return i =>
			{
				var r = parser( i );

				if ( r.WasSuccessful )
				{
					r.Value.SetPos( Position.FromInput( i ), r.Remainder.Position - i.Position );
					return Result.Success( r.Value, r.Remainder );
				}
				return r;
			};
		}
	}
}
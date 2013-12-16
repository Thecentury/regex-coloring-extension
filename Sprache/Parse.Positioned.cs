
namespace Sprache
{
	partial class Parse
	{
		/// <summary>
		/// Construct a parser that will set the position to the position-aware
		/// T on succsessful match.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="parser"></param>
		/// <returns></returns>
		public static Parser<T> Positioned<T>( this Parser<T> parser ) where T : IPositionAware
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

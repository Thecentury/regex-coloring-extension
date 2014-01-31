namespace RegexParsing
{
	public static class ImperativePositionAwareExtensions
	{
		public static PrimitiveRegexToken ToPrimitiveToken( this IImperativePositionAware positionAware,
			PrimitiveRegexTokenKind kind )
		{
			return new PrimitiveRegexToken( kind, positionAware.Position.Pos, positionAware.Length );
		}

		public static int GetEnd( this IImperativePositionAware positionAware )
		{
			return positionAware.Position.Pos + positionAware.Length;
		}
	}
}
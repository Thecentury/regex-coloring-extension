using System;
using System.Windows.Media;
using RegexParsing;

namespace RegexColoring.Extension
{
	public static class Colorizer
	{
		private const double DefaultTransparency = 0.4;

		public static Brush GetBrushForToken( PrimitiveRegexTokenKind kind )
		{
			var brush = GetBrushForTokenCore( kind ).WithTransparency( DefaultTransparency );
			return brush;
		}

		private static SolidColorBrush GetBrushForTokenCore( PrimitiveRegexTokenKind kind )
		{
			switch ( kind )
			{
				case PrimitiveRegexTokenKind.OpenSquareBracket:
				case PrimitiveRegexTokenKind.CloseSquareBracket:
				case PrimitiveRegexTokenKind.CharacterListNegation:
					return Brushes.Aquamarine;
				case PrimitiveRegexTokenKind.RepetitionsCount:
					return Brushes.CornflowerBlue;
				case PrimitiveRegexTokenKind.RangeStart:
				case PrimitiveRegexTokenKind.RangeSymbol:
				case PrimitiveRegexTokenKind.RangeEnd:
				case PrimitiveRegexTokenKind.LiteralCharacter:
					return Brushes.Orange;
				case PrimitiveRegexTokenKind.LiteralString:
					return Brushes.Transparent;
				case PrimitiveRegexTokenKind.GroupStart:
				case PrimitiveRegexTokenKind.GroupEnd:
					return Brushes.Thistle;
				case PrimitiveRegexTokenKind.NonCapturingGroup:
					return Brushes.CadetBlue;
				default:
					throw new ArgumentOutOfRangeException( "kind" );
			}
		}
	}
}
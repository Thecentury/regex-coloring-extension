using System.Windows.Media;

namespace RegexColoring.Extension
{
	public static class BrushExtensions
	{
		public static SolidColorBrush WithTransparency( this SolidColorBrush brush, double transparency )
		{
			SolidColorBrush modifiableClone = brush;
			if ( brush.IsFrozen )
			{
				modifiableClone = brush.Clone();
			}

			Color color = modifiableClone.Color;
			color.A = (byte)( transparency * 255 );
			modifiableClone.Color = color;

			return modifiableClone;
		}
	}
}

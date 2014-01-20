using Microsoft.VisualStudio.Text;

namespace RegexColoring.Extension
{
	public static class SpanExtensions
	{
		public static Span Shift( this Span span, int shift )
		{
			return new Span( span.Start + shift, span.Length );
		}
	}
}
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using RegexParsing;

namespace RegexColoring.Extension
{
	public static class SpanExtensions
	{
		public static Span Shift( this Span span, int shift )
		{
			return new Span( span.Start + shift, span.Length );
		}
	}

	///<summary>
	/// RegexColoring.Extension places red boxes behind all the "A"s in the editor window
	///</summary>
	public class RegexColoringExtension
	{
		private readonly IAdornmentLayer _layer;
		private readonly IWpfTextView _view;

		public RegexColoringExtension( IWpfTextView view )
		{
			_view = view;
			_layer = view.GetAdornmentLayer( "RegexColoring.Extension" );

			//Listen to any event that changes the layout (text changes, scrolling, etc)
			_view.LayoutChanged += OnLayoutChanged;
		}

		/// <summary>
		/// On layout change add the adornment to any reformatted lines
		/// </summary>
		private void OnLayoutChanged( object sender, TextViewLayoutChangedEventArgs e )
		{
			foreach ( ITextViewLine line in e.NewOrReformattedLines )
			{
				CreateVisuals( line );
			}
		}

		/// <summary>
		/// Within the given line add the scarlet box behind the a
		/// </summary>
		private void CreateVisuals( ITextViewLine line )
		{
			//grab a reference to the lines in the current TextView 
			IWpfTextViewLineCollection textViewLines = _view.TextViewLines;

			var lineFromPosition = line.Snapshot.GetLineFromPosition( line.Start );
			var text = lineFromPosition.GetText();

			string pattern;
			Span? patternSpan = RegexExtractor.TryExtractPatternSpan( text, out pattern );

			if ( patternSpan == null )
			{
				return;
			}

			int shift = line.Start.Position;
			Span shiftedSpan = patternSpan.Value.Shift( shift );

			SnapshotSpan span = new SnapshotSpan( _view.TextSnapshot, shiftedSpan );

			var result = RegexParser.TryParseRegex( pattern );
			if ( !result.WasSuccessful )
			{
				return;
			}
			var tokens = result.Value;

			var primitiveTokens = tokens.SelectMany( t => t.GetPrimitiveTokens() ).ToList();

			foreach ( var token in primitiveTokens )
			{
				var tokenSpan = new Span( token.Start, token.Length ).Shift( shiftedSpan.Start );
				Geometry geometry = textViewLines.GetMarkerGeometry( new SnapshotSpan( _view.TextSnapshot, tokenSpan ) );

				if ( geometry != null )
				{
					var brush = Colorizer.GetBrushForToken( token.Kind );
					GeometryDrawing drawing = new GeometryDrawing( brush, null, geometry );
					drawing.Freeze();

					DrawingImage drawingImage = new DrawingImage( drawing );
					drawingImage.Freeze();

					Image image = new Image { Source = drawingImage };

					//Align the image with the top of the bounds of the text geometry
					Canvas.SetLeft( image, geometry.Bounds.Left );
					Canvas.SetTop( image, geometry.Bounds.Top );

					_layer.AddAdornment( AdornmentPositioningBehavior.TextRelative, span, null, image, null );
				}
			}
		}
	}

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
				default:
					throw new ArgumentOutOfRangeException( "kind" );
			}
		}
	}
}

using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace RegexColoring.Extension
{
	///<summary>
	///RegexColoring.Extension places red boxes behind all the "A"s in the editor window
	///</summary>
	public class RegexColoringExtension
	{
		IAdornmentLayer _layer;
		IWpfTextView _view;
		Brush _brush;
		Pen _pen;

		public RegexColoringExtension( IWpfTextView view )
		{
			_view = view;
			_layer = view.GetAdornmentLayer( "RegexColoring.Extension" );

			//Listen to any event that changes the layout (text changes, scrolling, etc)
			_view.LayoutChanged += OnLayoutChanged;

			//Create the pen and brush to color the box behind the a's
			Brush brush = new SolidColorBrush( Color.FromArgb( 0x20, 0x00, 0x00, 0xff ) );
			brush.Freeze();
			Brush penBrush = new SolidColorBrush( Colors.Red );
			penBrush.Freeze();
			Pen pen = new Pen( penBrush, 0.5 );
			pen.Freeze();

			_brush = brush;
			_pen = pen;
		}

		/// <summary>
		/// On layout change add the adornment to any reformatted lines
		/// </summary>
		private void OnLayoutChanged( object sender, TextViewLayoutChangedEventArgs e )
		{
			foreach ( ITextViewLine line in e.NewOrReformattedLines )
			{
				this.CreateVisuals( line );
			}
		}

		/// <summary>
		/// Within the given line add the scarlet box behind the a
		/// </summary>
		private void CreateVisuals( ITextViewLine line )
		{
			//grab a reference to the lines in the current TextView 
			IWpfTextViewLineCollection textViewLines = _view.TextViewLines;
			int start = line.Start;
			int end = line.End;

			var lineFromPosition = line.Snapshot.GetLineFromPosition( line.Start );
			var text = lineFromPosition.GetText();

			string pattern;
			Span? patternSpan = RegexExtracter.TryExtractPatternSpan( text, out pattern );

			if ( patternSpan == null )
			{
				return;
			}

			int shift = line.Start.Position;
			Span shiftedSpan = Span.FromBounds( patternSpan.Value.Start + shift,
				patternSpan.Value.End + shift );

			SnapshotSpan span = new SnapshotSpan( _view.TextSnapshot, shiftedSpan );
			Geometry g = textViewLines.GetMarkerGeometry( span );
			if ( g != null )
			{
				GeometryDrawing drawing = new GeometryDrawing( _brush, _pen, g );
				drawing.Freeze();

				DrawingImage drawingImage = new DrawingImage( drawing );
				drawingImage.Freeze();

				Image image = new Image();
				image.Source = drawingImage;

				//Align the image with the top of the bounds of the text geometry
				Canvas.SetLeft( image, g.Bounds.Left );
				Canvas.SetTop( image, g.Bounds.Top );

				_layer.AddAdornment( AdornmentPositioningBehavior.TextRelative, span, null, image, null );
			}
		}
	}

	internal static class RegexExtracter
	{
		private static readonly Regex _regexPatternExtractor = new Regex( @"new\s*(?:System\.Text\.RegularExpressions\.|global::System\.Text\.RegularExpressions\.)?Regex\s*\(\s*""(?<pattern>.*)""\)", RegexOptions.Compiled );

		public static Span? TryExtractPatternSpan( string line, out string pattern )
		{
			Match match = _regexPatternExtractor.Match( line );

			if ( !match.Success )
			{
				pattern = null;
				return null;
			}

			var group = match.Groups["pattern"];
			pattern = @group.Value;
			return Span.FromBounds( group.Index, group.Index + group.Length );
		}
	}
}

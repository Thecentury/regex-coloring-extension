using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;

namespace RegexColoring.Extension
{
	internal static class RegexExtractor
	{
		private static readonly Regex _regexPatternExtractor = new Regex( @"new\s*(?:System\.Text\.RegularExpressions\.|global::System\.Text\.RegularExpressions\.)?Regex\s*\(\s*""(?<pattern>(?:[^""]|\"")*)""\s*[^)]*\)", RegexOptions.Compiled );

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
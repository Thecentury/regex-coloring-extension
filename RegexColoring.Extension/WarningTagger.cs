using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace RegexColoring.Extension
{
	public sealed class WarningTagger : ITagger<WarningTag>
	{
		private readonly ITextBuffer _buffer;

		public WarningTagger( ITextBuffer buffer )
		{
			_buffer = buffer;
		}

		public IEnumerable<ITagSpan<WarningTag>> GetTags( NormalizedSnapshotSpanCollection spans )
		{
			foreach ( var snapshotSpan in spans )
			{
				string text = snapshotSpan.GetText();
			}
			yield break;
		}

		public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
	}
}
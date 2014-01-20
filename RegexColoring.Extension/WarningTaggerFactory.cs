using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace RegexColoring.Extension
{
	[TagType( typeof( WarningTag ) ), ContentType( "text" ), Export( typeof( ITaggerProvider ) )]
	public class WarningTaggerFactory : ITaggerProvider
	{
		public ITagger<T> CreateTagger<T>( ITextBuffer buffer ) where T : ITag
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException( "buffer" );
			}
			return (ITagger<T>)(object)buffer.Properties.GetOrCreateSingletonProperty( () => new WarningTagger( buffer ) );
		}
	}
}
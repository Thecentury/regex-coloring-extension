﻿using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace RegexColoring.Extension
{
	#region Adornment Factory
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
	/// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
	/// </summary>
	[Export( typeof( IWpfTextViewCreationListener ) )]
	[ContentType( "text" )]
	[TextViewRole( PredefinedTextViewRoles.Document )]
	internal sealed class RegexColoringExtensionFactory : IWpfTextViewCreationListener
	{
		/// <summary>
		/// Defines the adornment layer for the adornment. This layer is ordered 
		/// after the selection layer in the Z-order
		/// </summary>
		[Export( typeof( AdornmentLayerDefinition ) )]
		[Name( "RegexColoring.Extension" )]
		[Order( After = PredefinedAdornmentLayers.Selection, Before = PredefinedAdornmentLayers.Text )]
		public AdornmentLayerDefinition EditorAdornmentLayer = null;

		/// <summary>
		/// Instantiates a RegexColoring.Extension manager when a textView is created.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
		public void TextViewCreated( IWpfTextView textView )
		{
			new RegexColoringExtension( textView );
		}
	}
	#endregion //Adornment Factory
}

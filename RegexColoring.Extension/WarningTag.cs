using Microsoft.VisualStudio.Text.Tagging;

namespace RegexColoring.Extension
{
	public sealed class WarningTag : IErrorTag, ITag
	{
		public WarningTag( string tooltip )
		{
			_tooltip = tooltip;
		}

		private readonly string _tooltip;

		public string ErrorType
		{
			get { return "compiler warning"; }
		}

		public object ToolTipContent
		{
			get { return _tooltip; }
		}
	}
}
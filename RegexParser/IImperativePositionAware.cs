using Sprache;

namespace RegexParsing
{
	/// <summary>
	/// An interface for objects that have a source <see cref="Position"/>.
	/// </summary>
	public interface IImperativePositionAware
	{
		/// <summary>
		/// Set the start <see cref="Position"/> and the matched length.
		/// </summary>
		/// <param name="startPos">The start position</param>
		/// <param name="length">The matched length.</param>
		void SetPos( Position startPos, int length );
	}
}
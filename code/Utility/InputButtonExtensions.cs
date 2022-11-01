namespace GvarJam.Utility;

/// <summary>
/// Extension class for <see cref="InputButton"/>.
/// </summary>
public static class InputButtonExtensions
{
	/// <summary>
	/// Returns the input glpyh of the provided <see cref="InputButton"/>.
	/// </summary>
	/// <param name="button">The input button to get the glyph from.</param>
	/// <param name="glyphStyle">The style of the glyph to get.</param>
	/// <param name="glyphSize">The size of the glyph to get.</param>
	/// <returns>The input glyph of the provided <see cref="InputButton"/>.</returns>
	public static Texture GetInputGlpyh( this InputButton button, GlyphStyle glyphStyle, InputGlyphSize glyphSize = InputGlyphSize.Medium )
	{
		return Input.GetGlyph( button, glyphSize, glyphStyle );
	}
}

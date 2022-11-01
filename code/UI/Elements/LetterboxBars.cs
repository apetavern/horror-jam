namespace GvarJam.UI.Elements;

/// <summary>
/// Some cinematic letter box bars.
/// </summary>
public sealed class LetterboxBars : Panel
{
	/// <summary>
	/// Initializes a default instance of <see cref="LetterboxBars"/>.
	/// </summary>
	public LetterboxBars()
	{
		Add.Panel( "bar top" );
		Add.Panel( "bar bottom" );
	}
}

namespace GvarJam.UI.Elements;

/// <summary>
/// Some cinematic letter box bars.
/// </summary>
public sealed class LetterboxBars : Panel
{
	public LetterboxBars()
	{
		Add.Panel( "bar top" );
		Add.Panel( "bar bottom" );
	}
}

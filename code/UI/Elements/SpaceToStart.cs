namespace GvarJam.UI.Elements;

/// <summary>
/// A black screen with space to start on it.
/// </summary>
public sealed class SpaceToStart : Panel
{
	public SpaceToStart()
	{
		Add.Label( "Press space to start", "Text" );
	}
}

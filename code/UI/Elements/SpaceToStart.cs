namespace GvarJam.UI.Elements;

/// <summary>
/// A black screen with space to start on it.
/// </summary>
public sealed class SpaceToStart : Panel
{
	public Panel? Overlay { get; set; }

	public SpaceToStart() 
	{
		Overlay = Add.Panel( "Overlay" );
		Overlay.Add.Label( "Press space to start", "Text" );
		Overlay.SetClass( "Display", true );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn pawn )
			return;
		
		Overlay.SetClass( "Hide", !pawn.AwaitingCutsceneInput );
	}
}

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

		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = GetInputHint( InputButton.Jump );
		Overlay.Add.Label( "TO START" );

		Overlay.SetClass( "Display", true );
	}

	private Texture GetInputHint( InputButton button )
	{
		return Input.GetGlyph( button, InputGlyphSize.Medium, GlyphStyle.Dark );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn pawn )
			return;

		Overlay.SetClass( "Hide", !pawn.AwaitingCutsceneInput );
	}
}

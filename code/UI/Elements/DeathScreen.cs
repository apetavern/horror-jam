namespace GvarJam.UI.Elements;

/// <summary>
/// The death screen
/// </summary>
public sealed class DeathScreen : Panel
{
	public Panel? Overlay { get; set; }

	public DeathScreen()
	{
		Overlay = Add.Panel( "Overlay" );

		Overlay.Add.Label( "YOU HAVE DIED", "Title" );

		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = GetInputHint( InputButton.Jump );
		Overlay.Add.Label( "TO RETRY" );

		Overlay.SetClass( "Display", true );
	}

	private Texture GetInputHint( InputButton button )
	{
		return Input.GetGlyph( button, InputGlyphSize.Medium, GlyphStyle.Light );
	}

	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn pawn )
			return;

		Overlay.SetClass( "Show", pawn.LifeState == LifeState.Dead );
	}
}

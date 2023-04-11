namespace GvarJam.UI.Elements;

/// <summary>
/// The screen that shows once the player is killed.
/// </summary>
public sealed class DeathScreen : Panel
{
	/// <summary>
	/// The overlay to display to the player.
	/// </summary>
	public Panel Overlay { get; set; }

	/// <summary>
	/// Initializes a default instance of <see cref="DeathScreen"/>.
	/// </summary>
	public DeathScreen()
	{
		Overlay = Add.Panel( "Overlay" );
		Overlay.Add.Label( "YOU HAVE DIED", "Title" );

		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = InputButton.Jump.GetInputGlpyh( GlyphStyle.Light );
		Overlay.Add.Label( "TO RETRY" );

		Overlay.SetClass( "Display", true );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		base.Tick();

		if ( Game.LocalPawn is not Pawn pawn )
			return;

		Overlay.SetClass( "Show", pawn.LifeState == LifeState.Dead );
	}
}

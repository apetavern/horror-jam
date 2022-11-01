namespace GvarJam.UI.Elements;

/// <summary>
/// A blank screen with space to start on it.
/// </summary>
public sealed class SpaceToStart : Panel
{
	/// <summary>
	/// The overlay to display to the player.
	/// </summary>
	public Panel Overlay { get; set; }

	/// <summary>
	/// Initializes a default instance of <see cref="SpaceToStart"/>.
	/// </summary>
	public SpaceToStart()
	{
		Overlay = Add.Panel( "Overlay" );

		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = InputButton.Jump.GetInputGlpyh( GlyphStyle.Dark );
		Overlay.Add.Label( "TO START" );

		Overlay.SetClass( "Display", true );
	}

	/// <inheritdoc/>
	public override void Tick()
	{
		base.Tick();

		if ( Local.Pawn is not Pawn pawn )
			return;

		Overlay.SetClass( "Hide", !pawn.AwaitingCutsceneInput );
	}
}

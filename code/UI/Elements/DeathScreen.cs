namespace GvarJam.UI.Elements;

public sealed class DeathScreen : Panel
{
	public Panel Overlay { get; }

	public DeathScreen()
	{
		Overlay = Add.Panel( "Overlay" );
		Overlay.Add.Label( "YOU HAVE DIED", "Title" );
		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = Input.GetGlyph( "Jump", InputGlyphSize.Medium, GlyphStyle.Light );
		Overlay.Add.Label( "TO RETRY" );
		Overlay.SetClass( "Display", true );
	}

	public override void Tick()
	{
		base.Tick();
		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		Overlay.SetClass( "Show", player.IsDead );
	}
}

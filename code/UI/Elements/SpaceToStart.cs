namespace GvarJam.UI.Elements;

public sealed class SpaceToStart : Panel
{
	public Panel Overlay { get; }

	public SpaceToStart()
	{
		Overlay = Add.Panel( "Overlay" );
		Overlay.Add.Label( "PRESS" );
		var image = Overlay.Add.Image();
		image.Texture = Input.GetGlyph( "Jump", InputGlyphSize.Medium, GlyphStyle.Dark );
		Overlay.Add.Label( "TO START" );
		Overlay.SetClass( "Display", true );
	}

	public override void Tick()
	{
		base.Tick();
		var player = Game.ActiveScene?.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		Overlay.SetClass( "Hide", !player.Cutscenes.AwaitingCutsceneInput );
	}
}

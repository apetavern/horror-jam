namespace GvarJam.UI.Elements;

internal class StaminaBar : Panel
{
	private float opacity = 1.0f;
	private Color BackgroundColor => new Color( 0, 0, 0 ).WithAlpha( 0.25f * opacity );
	private Color ForegroundColor => Color.FromRgb( 0xF2D933 ).WithAlpha( opacity );
	private TimeSince timeSinceStaminaFull = 0;

	public StaminaBar()
	{

	}

	public override void DrawBackground( ref RenderState state )
	{
		if ( Local.Pawn is not Pawn { Controller: MovementController controller } )
			return;

		//
		// Opacity transition
		//
		if ( controller.Stamina.AlmostEqual( 1 ) )
			opacity = opacity.LerpTo( 0f, 10f * Time.Delta );
		else
			opacity = opacity.LerpTo( 1, 25f * Time.Delta );

		//
		// Stamina icon
		//
		Graphics.DrawIcon( new Rect( Box.Rect.Position - new Vector2( 32, 14 ), 32 ), "bolt", ForegroundColor );

		//
		// Bar background
		//
		Graphics.DrawRoundedRectangle( Box.Rect, BackgroundColor );

		//
		// Bar fill / foreground
		//
		var rect = new Rect( Box.Rect.Position, Box.Rect.Size * new Vector2( controller.Stamina, 1 ) );
		Graphics.DrawRoundedRectangle( rect, ForegroundColor );
	}

	public override void Tick()
	{
		base.Tick();
	}
}

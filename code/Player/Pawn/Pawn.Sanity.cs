namespace GvarJam.Player;

public partial class Pawn
{
	[ConVar.Client( "SanityPostEnabled" )]
	public bool SanityPostEnabled { get; set; } = false;

	/// <summary>
	/// The current sanity level of the player.
	/// </summary>
	[Net, Predicted]
	public float InsanityLevel { get; set; } = 0;

	private float sanityDrainPerSecond { get; set; } = 0.02f;

	private float sanityGainPerSecond { get; set; } = 0.08f;

	public void SimulateSanity()
	{
		DebugOverlay.ScreenText( $"Insanity level: {InsanityLevel}" );

		if ( Lamp is not null && !Lamp.Enabled )
			InsanityLevel += sanityDrainPerSecond / Global.TickRate;
		else
			InsanityLevel -= sanityGainPerSecond / Global.TickRate;

		if ( Host.IsClient && SanityPostEnabled && Local.Pawn is Pawn pawn )
		{
			pawn.ApplyVignetteAmount( InsanityLevel.Clamp( 0.2f, 0.70f ) );
			pawn.ApplyBrightnessAmount( 1 - InsanityLevel.Clamp( 0f, 0.90f ) );
			pawn.ApplyFilmGrain( (InsanityLevel / 50).Clamp( 0, 0.02f ) );

			if ( InsanityLevel > 0.90f )
				pawn.ApplyMotionBlur( InsanityLevel.Clamp( 0, 0.25f ) );
			else
				pawn.ApplyMotionBlur( 0 );
		}
			

		InsanityLevel = InsanityLevel.Clamp( 0, 1 );
	}
}

namespace GvarJam.Player;

partial class Pawn
{
	/// <summary>
	/// The amount of sanity to drain per second.
	/// </summary>
	private const float SanityDrainPerSecond = 0.02f;

	/// <summary>
	/// The amount fo sanity to gain per second.
	/// </summary>
	private const float SanityGainPerSecond = 0.08f;

	/// <summary>
	/// Whether or not the post processing for the sanity system is enabled.
	/// </summary>
	[ConVar.Client( "SanityPostEnabled" )]
	public bool SanityPostEnabled { get; set; } = true;

	/// <summary>
	/// The current sanity level of the player.
	/// </summary>
	[Net, Predicted]
	public float InsanityLevel { get; set; } = 0;

	/// <summary>
	/// Simulates the sanity system.
	/// </summary>
	public void SimulateSanity()
	{
		if ( HorrorGame.Debug )
			DebugOverlay.ScreenText( $"Insanity level: {InsanityLevel}" );

		if ( Lamp is not null && !Lamp.Enabled )
			InsanityLevel += SanityDrainPerSecond / Global.TickRate;
		else
			InsanityLevel -= SanityGainPerSecond / Global.TickRate;

		// Subtract insanity if lights are found nearby.
		if ( GetLightsInSphere( 200 ) > 0 )
			InsanityLevel -= SanityGainPerSecond;

		if ( IsClient && SanityPostEnabled )
		{
			ApplyVignetteAmount( InsanityLevel.Clamp( 0.2f, 0.80f ) );
			ApplyBrightnessAmount( 1 - InsanityLevel.Clamp( 0f, 0.90f ) );
			ApplyFilmGrain( (InsanityLevel / 50).Clamp( 0, 0.02f ) );

			if ( InsanityLevel > 0.90f )
				ApplyMotionBlur( InsanityLevel.Clamp( 0, 0.25f ) );
			else
				ApplyMotionBlur( 0 );
		}

		InsanityLevel = InsanityLevel.Clamp( 0, 1 );
	}

	private int GetLightsInSphere( int radius )
	{
		var lights = All.OfType<PointLightEntity>().Where( x => x.Enabled && Vector3.DistanceBetween( x.Position, Position ) < radius );

		return lights.Count();
	}
}

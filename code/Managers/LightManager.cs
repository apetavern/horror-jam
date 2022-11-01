namespace GvarJam.Managers;

/// <summary>
/// The light manager for the game.
/// </summary>
public static class LightManager
{
	/// <summary>
	/// Whether or not the lights should be flickering.
	/// </summary>
	private static bool ShouldLightsFlicker { get; set; }
	/// <summary>
	/// The time in seconds between flicks of the lighting.
	/// </summary>
	private static float TimeBetweenFlicker { get; set; } = 1;
	/// <summary>
	/// The time since the last light flicker.
	/// </summary>
	private static TimeSince TimeSinceLastFlicker { get; set; }

	/// <summary>
	/// Sets the state of the lights in the ship.
	/// </summary>
	/// <param name="shouldBeOn"></param>
	public static void SetLightState( bool shouldBeOn )
	{
		if ( HorrorGame.Current.LightsEnabled == shouldBeOn )
			return;

		// Dirty hack to change material group of all light models.
		var allLightModelEnts = Entity.All.OfType<ModelEntity>().Where( x => x.Model is not null && x.Model.ResourceName.ToLower().Contains( "light" ) );

		foreach ( var model in allLightModelEnts ) 
			model?.SetMaterialGroup( shouldBeOn ? 0 : 1 );

		var allLights = Entity.All.OfType<PointLightEntity>();

		foreach ( var light in allLights )
			light.Enabled = shouldBeOn;

		if ( shouldBeOn )
			Sound.FromScreen( "generator_power_on" );
		else
			Sound.FromScreen( "generator_power_off" );

		HorrorGame.Current.LightsEnabled = shouldBeOn;
	}

	/// <summary>
	/// Turn the lighting on / off from a server sided environment.
	/// </summary>
	/// <param name="shouldBeOn"></param>
	[ConCmd.Server]
	public static void ToggleLighting( bool shouldBeOn )
	{
		SetLightState( shouldBeOn );
	}

	/// <summary>
	/// Call in a server sided environment.
	/// </summary>
	/// <param name="shouldFlicker">Enable or disable the flicker.</param>
	/// <param name="timeBetweenFlick">How fast the light should flash on and off.</param>
	public static void FlickerLighting( bool shouldFlicker, float timeBetweenFlick )
	{
		ShouldLightsFlicker = shouldFlicker;
		TimeBetweenFlicker = timeBetweenFlick;

		if ( !shouldFlicker )
		{
			SetLightState( true );
		}
	}

	/// <summary>
	/// Ticks the ships lighting.
	/// </summary>
	[Event.Tick.Server]
	public static void Tick()
	{
		if ( ShouldLightsFlicker && TimeSinceLastFlicker > TimeBetweenFlicker )
		{
			var allLights = Entity.All.OfType<PointLightEntity>();

			foreach ( var light in allLights )
				light.Enabled = !light.Enabled;

			TimeSinceLastFlicker = 0;
		}
	}
}

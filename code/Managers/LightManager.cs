namespace GvarJam.Managers;

/// <summary>
/// The light manager for the game.
/// </summary>
public static class LightManager
{
	static bool shouldLightsFlicker { get; set; }

	static float timeBetweenFlicker { get; set; } = 1;

	static TimeSince timeSinceLastFlicker { get; set; }

	static void SetLightState( bool shouldBeOn )
	{
		if ( (Game.Current as HorrorGame).areLightsOn == shouldBeOn )
			return;

		// Dirty hack to change material group of all light models.
		var allLightModelEnts = Entity.All.OfType<ModelEntity>();

		foreach ( var model in allLightModelEnts )
			model?.SetMaterialGroup( shouldBeOn ? 0 : 1 );

		var allLights = Entity.All.OfType<PointLightEntity>();

		foreach ( var light in allLights )
			light.Enabled = shouldBeOn;

		if ( shouldBeOn )
			Sound.FromScreen( "generator_power_on" );
		else
			Sound.FromScreen( "generator_power_off" );

		(Game.Current as HorrorGame).areLightsOn = shouldBeOn;
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
		shouldLightsFlicker = shouldFlicker;
		timeBetweenFlicker = timeBetweenFlick;

		if ( !shouldFlicker )
		{
			SetLightState( true );
		}
	}

	[Event.Tick.Server]
	public static void Tick()
	{
		if ( shouldLightsFlicker && timeSinceLastFlicker > timeBetweenFlicker )
		{
			var allLights = Entity.All.OfType<PointLightEntity>();

			foreach ( var light in allLights )
				light.Enabled = !light.Enabled;

			timeSinceLastFlicker = 0;
		}
	}
}

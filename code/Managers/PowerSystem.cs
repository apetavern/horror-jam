namespace GvarJam.Managers;

public sealed class PowerSystem : GameObjectSystem<PowerSystem>
{
	public bool LightsEnabled { get; private set; } = true;

	private bool _shouldFlicker;
	private float _timeBetweenFlicker = 1f;
	private TimeSince _timeSinceLastFlicker;

	public PowerSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.UpdateBones, 0, TickFlicker, "PowerSystemFlicker" );
	}

	public void AttachLightCullers()
	{
		foreach ( var light in Scene.GetAllComponents<PointLight>() )
		{
			light.GameObject.GetOrAddComponent<LightCullComponent>();
		}
	}

	public void SetLightState( bool shouldBeOn )
	{
		if ( LightsEnabled == shouldBeOn )
			return;

		foreach ( var mr in Scene.GetAllComponents<ModelRenderer>() )
		{
			if ( mr.Model is not null && mr.Model.Name.Contains( "light", StringComparison.OrdinalIgnoreCase ) )
			{
				mr.MaterialGroup = shouldBeOn ? "default" : "Off";
			}
		}

		foreach ( var light in Scene.GetAllComponents<PointLight>() )
		{
			light.Enabled = shouldBeOn;
		}

		AudioDirector.Play2D( shouldBeOn ? "generator_power_on" : "generator_power_off" );
		LightsEnabled = shouldBeOn;
	}

	public void FlickerLighting( bool shouldFlicker, float timeBetweenFlick )
	{
		_shouldFlicker = shouldFlicker;
		_timeBetweenFlicker = timeBetweenFlick;

		if ( !shouldFlicker )
			SetLightState( true );
	}

	private void TickFlicker()
	{
		if ( !_shouldFlicker || _timeSinceLastFlicker <= _timeBetweenFlicker )
			return;

		foreach ( var light in Scene.GetAllComponents<PointLight>() )
		{
			light.Enabled = !light.Enabled;
		}

		_timeSinceLastFlicker = 0;
	}

	[ConCmd( "toggle_lighting" )]
	public static void ToggleLighting( bool shouldBeOn )
	{
		Game.ActiveScene?.GetSystem<PowerSystem>()?.SetLightState( shouldBeOn );
	}
}

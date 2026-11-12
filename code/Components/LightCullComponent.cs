namespace GvarJam.Components;

public sealed class LightCullComponent : Component
{
	private PointLight _light;
	private Color _baseColor;
	private float _brightnessFade = 1f;

	protected override void OnStart()
	{
		_light = GetComponent<PointLight>();
		if ( _light is null ) return;

		_baseColor = _light.LightColor;
		_light.Shadows = false;
	}

	protected override void OnUpdate()
	{
		if ( _light is null ) return;

		var power = Scene?.GetSystem<PowerSystem>();
		if ( power is not null && !power.LightsEnabled )
		{
			if ( _light.Enabled )
				_light.Enabled = false;
			return;
		}

		var player = Scene.GetAllComponents<ColdOpenPlayer>().FirstOrDefault();
		if ( player is null ) return;

		MountedCameraComponent viewCam = null;
		foreach ( var cam in Scene.GetAllComponents<MountedCameraComponent>() )
		{
			if ( cam.IsBeingViewed )
			{
				viewCam = cam;
				break;
			}
		}

		float dist = Vector3.DistanceBetween( WorldPosition, player.WorldPosition + player.WorldRotation.Forward * 128f );

		bool occluded = Scene.Trace.Ray( WorldPosition, player.WorldPosition + Vector3.Up * 45f ).IgnoreGameObjectHierarchy( player.GameObject ).Run().Hit
			&& Scene.Trace.Ray( WorldPosition, player.EyePosition ).IgnoreGameObjectHierarchy( player.GameObject ).Run().Hit;

		if ( viewCam is not null )
		{
			occluded = occluded && Scene.Trace.Ray( WorldPosition, viewCam.WorldPosition - Vector3.Up * 45f ).IgnoreGameObjectHierarchy( player.GameObject ).Run().Hit;
		}

		if ( occluded )
			_brightnessFade = 0f;

		if ( !occluded || dist < 256f )
			_brightnessFade = 1f;

		float currentScale = (_baseColor.r + _baseColor.g + _baseColor.b) > 0.0001f
			? (_light.LightColor.r + _light.LightColor.g + _light.LightColor.b) / (_baseColor.r + _baseColor.g + _baseColor.b)
			: (_light.Enabled ? 1f : 0f);

		float nextScale = MathX.Lerp( currentScale, _brightnessFade, 0.5f * Time.Delta );
		_light.LightColor = _baseColor * nextScale;

		if ( nextScale <= 0.01f )
		{
			_light.Shadows = false;
			_light.Enabled = false;
		}
		else
		{
			_light.Shadows = true;
			_light.Enabled = true;
		}
	}
}

namespace GvarJam.Player;

public sealed class PlayerCameraComponent : Component
{
	private const float ModeSwitchSpeed = 2.0f;
	private const float LeanDegrees = 0.5f;
	private const float LeanMax = LeanDegrees * 2f;
	private const float LeanSmooth = 15.0f;
	private const float MaxBobSpeed = 320f;
	private const float FovIncrease = 5f;
	private const float FovSpeed = 10f;
	private const float BobStrength = 15f;

	[Sync] public ViewModeType ViewMode { get; private set; } = ViewModeType.FirstPerson;

	public CameraComponent ActiveCamera { get; private set; }

	private ColdOpenPlayer _player;
	private GameObject _cameraGo;
	private float _modeSwitchProgress = 1f;
	private Vector3 _lastPos;
	private bool _shouldOverrideTransform;
	private Transform _overrideTransform;

	private float _bobbing;
	private float _roll;
	private float _fovDelta;

	protected override void OnStart()
	{
		_player = GetComponent<ColdOpenPlayer>();

		_cameraGo = new GameObject( true, "PlayerCamera" );
		_cameraGo.SetParent( GameObject, false );
		ActiveCamera = _cameraGo.AddComponent<CameraComponent>();
		ActiveCamera.IsMainCamera = true;
		ActiveCamera.Priority = 10;
		ActiveCamera.ZNear = 10f;
		ActiveCamera.ZFar = 10000f;
		ActiveCamera.FieldOfView = 90f;
		ActiveCamera.FovAxis = CameraComponent.Axis.Vertical;

		_cameraGo.AddComponent<Bloom>();
		_cameraGo.AddComponent<Tonemapping>();

		_lastPos = _player.EyePosition;
		ActiveCamera.WorldPosition = _lastPos;
		ActiveCamera.WorldRotation = _player.EyeRotation;
	}

	protected override void OnUpdate()
	{
		if ( _player is null || ActiveCamera is null ) return;

		_modeSwitchProgress += Time.Delta;

		Vector3 targetPos = ViewMode switch
		{
			ViewModeType.FirstPerson => UpdateFirstPerson(),
			ViewModeType.ThirdPerson => UpdateThirdPerson(),
			_ => _player.EyePosition
		};

		Rotation targetRot = _player.EyeRotation;

		if ( _shouldOverrideTransform )
		{
			targetPos = _overrideTransform.Position;
			targetRot = _overrideTransform.Rotation;
		}

		ActiveCamera.WorldPosition = _shouldOverrideTransform
			? targetPos
			: ActiveCamera.WorldPosition.LerpTo( targetPos, (_modeSwitchProgress * ModeSwitchSpeed).Clamp( 0f, 1f ) );
		ActiveCamera.WorldRotation = targetRot;
		ActiveCamera.FieldOfView = 90f;
		ActiveCamera.ZNear = (_player.Cutscenes?.InCutscene ?? false) ? 10f : 4f;

		if ( !_shouldOverrideTransform )
			ApplyCameraEffects();
	}

	private void ApplyCameraEffects()
	{
		var speed = _player.Velocity.Length.LerpInverse( 0f, MaxBobSpeed );
		var forwardSpeed = Vector3.Dot( _player.Velocity.Normal, ActiveCamera.WorldRotation.Forward );

		var left = ActiveCamera.WorldRotation.Left;
		var up = ActiveCamera.WorldRotation.Up;

		if ( _player.IsGrounded )
			_bobbing += Time.Delta * BobStrength * speed;

		var targetRoll = Vector3.Dot( _player.Velocity, ActiveCamera.WorldRotation.Right ) / 180f;
		targetRoll += MathF.Sin( _bobbing / 2f ) * LeanDegrees;
		targetRoll = targetRoll.Clamp( -LeanMax, LeanMax );
		_roll = _roll.LerpTo( targetRoll, Time.Delta * LeanSmooth );

		_fovDelta = _fovDelta.LerpTo( speed * FovIncrease * MathF.Abs( forwardSpeed ), Time.Delta * FovSpeed );

		float x = MathF.Sin( _bobbing * 0.5f ) * speed;
		float y = MathF.Sin( _bobbing ) * speed;
		ActiveCamera.WorldPosition += left * x + up * y;
		ActiveCamera.WorldRotation *= Rotation.From( 0, 0, _roll );
		ActiveCamera.FieldOfView = 90f + _fovDelta;

		var hud = Scene.GetAllComponents<HudPanelComponent>().FirstOrDefault();
		if ( hud?.Panel is not null )
		{
			var tx = new PanelTransform();
			tx.AddTranslate( x, y );
			hud.Panel.Style.Transform = tx;
			hud.Panel.Style.Dirty();
		}
	}

	public void OverrideTransform( Transform transform )
	{
		_shouldOverrideTransform = true;
		_overrideTransform = transform;
	}

	public void DisableTransformOverride()
	{
		_shouldOverrideTransform = false;
	}

	public float GetPlayerAlpha()
	{
		float baseAlpha = (_modeSwitchProgress * ModeSwitchSpeed * 4.0f).Clamp( 0f, 1f );
		return ViewMode switch
		{
			ViewModeType.FirstPerson => 1.0f - baseAlpha,
			ViewModeType.ThirdPerson => baseAlpha,
			_ => 1.0f
		};
	}

	public void GoToFirstPerson()
	{
		if ( ViewMode == ViewModeType.FirstPerson ) return;
		ViewMode = ViewModeType.FirstPerson;
		_modeSwitchProgress = 0f;
	}

	public void GoToThirdPerson()
	{
		if ( ViewMode == ViewModeType.ThirdPerson ) return;
		ViewMode = ViewModeType.ThirdPerson;
		_modeSwitchProgress = 0f;
	}

	private Vector3 UpdateThirdPerson()
	{
		var targetPos = _player.EyePosition
			+ _player.EyeRotation.Backward * 64f
			+ _player.EyeRotation.Right * 16f;

		var tr = Scene.Trace.Ray( _player.EyePosition, targetPos )
			.IgnoreGameObjectHierarchy( _player.GameObject )
			.WithoutTags( "camignore", "trigger" )
			.Radius( 16f )
			.Run();

		return InterpolatePosition( tr.EndPosition );
	}

	private Vector3 UpdateFirstPerson()
	{
		return InterpolatePosition( _player.EyePosition ) + GetAdditiveNoise();
	}

	private Vector3 InterpolatePosition( Vector3 targetPos )
	{
		if ( targetPos.Distance( _lastPos ) < 300f )
			targetPos = Vector3.Lerp( targetPos.WithZ( _lastPos.z ), targetPos, (20.0f * Time.Delta).Clamp( 0f, 1f ) );

		_lastPos = targetPos;
		return targetPos;
	}

	private float GetNoise( float offset, float speed )
	{
		var noise = Noise.Perlin( offset, offset, Time.Now * speed );
		return (noise - 0.5f) * 2f;
	}

	private Vector3 GetAdditiveNoise()
	{
		var rot = ActiveCamera.WorldRotation;
		return (GetNoise( 0.1f, 8f ) * rot.Up
			+ GetNoise( 5.5f, 1f ) * rot.Forward
			+ GetNoise( 16f, 8f ) * rot.Right) * 4f;
	}
}

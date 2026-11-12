namespace GvarJam.HammerEntities;

public sealed class CameraControllerComponent : LockedUseComponent
{
	public override string LegacyTypeName => "CameraController";

	[Sync] public TimeSince TimeSinceUsed { get; set; } = 10f;
	public MountedCameraComponent TargetCamera { get; private set; }
	public int NumberOfUsableCameras { get; private set; }

	private int _currentCameraIndex;
	public int CurrentCameraIndex
	{
		get => _currentCameraIndex;
		set
		{
			if ( TargetCamera.IsValid() )
				TargetCamera.IsBeingViewed = false;

			var cameras = FindUsableMountedCameras();
			if ( cameras.Count == 0 ) return;

			_currentCameraIndex = ((value % cameras.Count) + cameras.Count) % cameras.Count;
			TargetCamera = cameras[_currentCameraIndex];
			TargetCamera.IsBeingViewed = true;

			_zoneLabel?.UpdateName( TargetCamera.ZoneName.ToUpper() );
			AudioDirector.PlayAt( "joystick_click", WorldPosition );
		}
	}

	private SkinnedModelRenderer _consoleRenderer;
	private GameObject _screenGo;
	private ModelRenderer _screenRenderer;
	private CameraComponent _feedCamera;
	private Texture _renderTexture;
	private Material _dynamicMonitorMaterial;
	private CameraControllerPanelComponent _zoneLabel;
	private GameObject _zonePanelGo;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Camera Controller";

		var model = Model.Load( "models/cameraconsole/console.vmdl" );
		_consoleRenderer = GameObject.GetOrAddComponent<SkinnedModelRenderer>();
		_consoleRenderer.Model = model;
		_consoleRenderer.CreateAttachments = true;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;

		_screenGo = new GameObject( true, "ConsoleScreen" );
		_screenGo.SetParent( GameObject, false );
		_screenRenderer = _screenGo.AddComponent<ModelRenderer>();
		_screenRenderer.Model = Model.Load( "models/cameraconsole/console_screen.vmdl" );
		_screenRenderer.MaterialGroup = "standby";
	}

	public override void SimulateLocked( ColdOpenPlayer player )
	{
		if ( _consoleRenderer is not null && player.BodyRenderer.IsValid() )
		{
			var rAttach = _consoleRenderer.GetAttachmentObject( "rhand_attach" );
			if ( rAttach is not null )
				player.BodyRenderer.Set( "right_hand_ik", rAttach.WorldTransform );

			var lAttach = _consoleRenderer.GetAttachmentObject( "lhand_attach" );
			if ( lAttach is not null )
				player.BodyRenderer.Set( "left_hand_ik", lAttach.WorldTransform );
		}

		if ( TimeSinceUsed < 1f ) return;

		if ( Input.Released( "Use" ) )
		{
			if ( TargetCamera.IsValid() )
				TargetCamera.IsBeingViewed = false;
			StopUsing();
			return;
		}

		if ( Input.Released( "Right" ) )
		{
			_consoleRenderer?.Set( "right", true );
			CurrentCameraIndex = _currentCameraIndex + 1;
			return;
		}

		if ( Input.Released( "Left" ) )
		{
			_consoleRenderer?.Set( "left", true );
			CurrentCameraIndex = _currentCameraIndex - 1;
			return;
		}
	}

	protected override void OnUpdate()
	{
		if ( _feedCamera is null || !TargetCamera.IsValid() )
			return;

		var lens = TargetCamera.LensAttachment;
		if ( lens is not null )
		{
			_feedCamera.WorldPosition = lens.WorldPosition;
			_feedCamera.WorldRotation = lens.WorldRotation;
		}
		else
		{
			_feedCamera.WorldPosition = TargetCamera.WorldPosition;
			_feedCamera.WorldRotation = TargetCamera.WorldRotation;
		}

		_feedCamera.FieldOfView = TargetCamera.Fov;
		_feedCamera.ZNear = TargetCamera.ZNear;
		_feedCamera.ZFar = TargetCamera.ZFar;
	}

	protected override void OnDestroy()
	{
		StopUsing();
		base.OnDestroy();
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		user.Interaction.InteractedEntity = this;

		if ( _consoleRenderer is not null && user.BodyRenderer.IsValid() )
		{
			if ( _consoleRenderer.GetAttachmentObject( "rhand_attach" ) is not null )
				user.BodyRenderer.Set( "b_IKright", true );
			if ( _consoleRenderer.GetAttachmentObject( "lhand_attach" ) is not null )
				user.BodyRenderer.Set( "b_IKleft", true );
		}

		SnapPlayerToUsePosition( user, 45f );
		TimeSinceUsed = 0;
		User = user;

		StartFeed();
	}

	private void StartFeed()
	{
		if ( _renderTexture is null )
			_renderTexture = Texture.CreateRenderTarget( "CameraMonitor", ImageFormat.RGBA8888, new Vector2( 512, 512 ) );

		if ( _dynamicMonitorMaterial is null )
		{
			var baseMat = Material.Load( "models/cameraconsole/materials/monitor.vmat" );
			_dynamicMonitorMaterial = baseMat?.CreateCopy();
			_dynamicMonitorMaterial?.Set( "g_tColor", _renderTexture );
		}

		if ( _screenRenderer.IsValid() && _dynamicMonitorMaterial is not null )
		{
			_screenRenderer.MaterialOverride = _dynamicMonitorMaterial;
			_screenRenderer.Attributes.Set( "g_tColor", _renderTexture );
			_screenRenderer.Tint = Color.White;
		}

		if ( _feedCamera is null )
		{
			var camGo = new GameObject( true, "ConsoleFeedCamera" );
			camGo.SetParent( GameObject, false );
			_feedCamera = camGo.AddComponent<CameraComponent>();
			_feedCamera.IsMainCamera = false;
			_feedCamera.Priority = -10;
			_feedCamera.RenderTarget = _renderTexture;
		}
		_feedCamera.Enabled = true;

		var zoneAttach = _consoleRenderer?.GetAttachmentObject( "zonename" );
		if ( zoneAttach is not null && _zonePanelGo is null )
		{
			_zonePanelGo = new GameObject( true, "ZoneNamePanel" );
			_zonePanelGo.SetParent( GameObject, true );
			_zonePanelGo.WorldTransform = zoneAttach.WorldTransform;
			var wp = _zonePanelGo.AddComponent<Sandbox.WorldPanel>();
			wp.PanelSize = new Vector2( 500, 75 );
			_zoneLabel = _zonePanelGo.AddComponent<CameraControllerPanelComponent>();
		}

		CurrentCameraIndex = 0;
	}

	public override void StopUsing()
	{
		if ( User is not null && User.IsValid() )
		{
			User.Interaction.InteractedEntity = null;
			if ( User.BodyRenderer.IsValid() )
			{
				User.BodyRenderer.Set( "b_IKleft", false );
				User.BodyRenderer.Set( "b_IKright", false );
			}
			ReleasePlayer( User );
		}

		if ( _feedCamera.IsValid() )
			_feedCamera.Enabled = false;

		if ( _screenRenderer.IsValid() )
		{
			_screenRenderer.MaterialOverride = null;
			_screenRenderer.MaterialGroup = "standby";
		}

		if ( _zonePanelGo.IsValid() )
		{
			_zonePanelGo.Destroy();
			_zonePanelGo = null;
			_zoneLabel = null;
		}

		User = null;
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return User is null && base.IsUsable( user );
	}

	public override bool OnUse( ColdOpenPlayer user )
	{
		OnUsed( user );
		return true;
	}

	private List<MountedCameraComponent> FindUsableMountedCameras()
	{
		var cameras = Scene.GetAllComponents<MountedCameraComponent>().Where( x => x.IsViewable ).ToList();
		NumberOfUsableCameras = cameras.Count - 1;
		return cameras;
	}
}

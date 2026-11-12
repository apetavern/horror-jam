namespace GvarJam.Player;

public sealed class HelmetLampComponent : Component
{
	private const float LampMaxPower = 100f;
	private const float LampDischargePerTick = 0.05f;

	private ColdOpenPlayer _player;
	private GameObject _helmetGo;
	private ModelRenderer _helmetRenderer;
	private SpotLight _lamp;
	private GameObject _lightAttach;

	[Sync] public float LampPower { get; set; } = 0f;
	[Sync] public TimeSince TimeSinceLampOff { get; private set; }
	[Sync] private bool _batteryInsertedProp { get; set; } = true;

	public bool HasHelmet => _helmetGo.IsValid();
	public float BatteryPercentage => LampPower / LampMaxPower;

	public bool LampEnabled
	{
		get => _lamp.IsValid() && _lamp.Enabled;
		private set
		{
			if ( !_lamp.IsValid() || LampEnabled == value ) return;
			_lamp.Enabled = value;
			if ( !value )
			{
				TimeSinceLampOff = 0;
				AudioDirector.PlayAt( "flashlight_click_off", _player.WorldPosition );
			}
			else
			{
				AudioDirector.PlayAt( "flashlight_click_on", _player.WorldPosition );
			}
		}
	}

	public bool BatteryInserted
	{
		get => _batteryInsertedProp;
		set
		{
			if ( !HasHelmet || _batteryInsertedProp == value ) return;
			_batteryInsertedProp = value;
			if ( value )
			{
				LampPower = LampMaxPower;
				_helmetRenderer?.SetBodyGroup( "battery", 0 );
			}
			else
			{
				LampEnabled = false;
				LampPower = 0f;
				_helmetRenderer?.SetBodyGroup( "battery", 1 );
			}
		}
	}

	protected override void OnAwake()
	{
		_player = GetComponent<ColdOpenPlayer>();
	}

	public void EquipHelmet()
	{
		_player ??= GetComponent<ColdOpenPlayer>();
		if ( HasHelmet ) return;
		_helmetGo = new GameObject( true, "Helmet" );
		_helmetGo.SetParent( _player.BodyRenderer.GameObject, false );
		_helmetRenderer = _helmetGo.AddComponent<SkinnedModelRenderer>();
		_helmetRenderer.Model = Model.Load( "models/cosmetics/spacehelmet.vmdl" );
		_helmetRenderer.CreateAttachments = true;
		if ( _helmetRenderer is SkinnedModelRenderer smr )
			smr.BoneMergeTarget = _player.BodyRenderer;

		_player.RegisterHideableRenderer( _helmetRenderer );

		_lightAttach = _helmetRenderer.GetAttachmentObject( "light" );
		var lampGo = new GameObject( true, "HelmetSpotLight" );
		lampGo.SetParent( _player.GameObject, false );
		_lamp = lampGo.AddComponent<SpotLight>();
		_lamp.Cookie = Texture.Load( "materials/effects/lightcookie.vtex" );
		_lamp.Enabled = false;
		_lamp.ConeOuter = 27f; // 45 * 0.6
		_lamp.ConeInner = 6f;  // 30 * 0.2
		_lamp.Radius = 1024f;
		_lamp.LightColor = Color.FromBytes( 181, 177, 255 );
		SyncLampTransform();
	}

	public void SyncLampTransform()
	{
		if ( !_lamp.IsValid() || _player is null ) return;

		bool firstPerson = _player.Camera?.ViewMode == ViewModeType.FirstPerson;
		_lamp.WorldPosition = (!firstPerson && _lightAttach.IsValid())
			? _lightAttach.WorldPosition
			: _player.EyePosition + _player.EyeRotation.Forward * 4f;
		_lamp.WorldRotation = _player.EyeRotation;
	}

	protected override void OnPreRender()
	{
		SyncLampTransform();
	}

	protected override void OnFixedUpdate()
	{
		if ( !HasHelmet || !_lamp.IsValid() ) return;

		if ( LampEnabled )
			LampPower = MathF.Max( 0f, LampPower - LampDischargePerTick );

		if ( LampPower <= 0f && LampEnabled )
			LampEnabled = false;

		_lamp.LightColor = Color.FromBytes( 181, 177, 255 ) * (BatteryPercentage * 1.5f);
		if ( _helmetRenderer.IsValid() )
			_helmetRenderer.Tint = Color.FromBytes( 181, 177, 255 ).WithAlpha( _helmetRenderer.Tint.a );
	}

	protected override void OnUpdate()
	{
		if ( !HasHelmet || !_lamp.IsValid() ) return;

		if ( Input.Pressed( "Flashlight" ) && LampPower > 0f )
			LampEnabled = !LampEnabled;
	}
}

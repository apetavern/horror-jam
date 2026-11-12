namespace GvarJam.HammerEntities;

public sealed class DoorControllerComponent : InteractableComponent
{
	public override string LegacyTypeName => "DoorEntity";

	[Property] public ItemType ItemRequiredToOpen { get; set; }
	[Property] public bool UseOpens { get; set; } = true;
	[Property] public bool StartLocked { get; set; } = false;
	[Property, Sync] public bool Locked { get; set; } = true;
	[Property, Sync] public bool ObjectiveLocked { get; set; } = false;
	[Property, Sync] public bool BypassItemRequirement { get; set; } = false;

	[Property] public int MoveDirType { get; set; } // 0=Moving, 1=Rotating, 2=AnimatingOnly
	[Property] public Angles MoveDir { get; set; }
	[Property] public bool MoveDirIsLocal { get; set; } = true;
	[Property] public float Distance { get; set; }
	[Property] public float InitialPosition { get; set; }
	[Property] public bool OpenAwayFromPlayer { get; set; }
	[Property] public float Speed { get; set; } = 100f;
	[Property] public float TimeBeforeReset { get; set; } = 4f;
	[Property] public string OtherDoorsToOpen { get; set; } = "";

	[Property] public string OpenSound { get; set; } = "";
	[Property] public string FullyOpenSound { get; set; } = "";
	[Property] public string CloseSound { get; set; } = "";
	[Property] public string FullyClosedSound { get; set; } = "";
	[Property] public string LockedSound { get; set; } = "";
	[Property] public string MovingSound { get; set; } = "";

	[Sync] public TimeSince TimeSinceUsed { get; set; } = 10f;

	private Sandbox.Mapping.Door _door;
	private SkinnedModelRenderer _skinned;
	private bool _animOpen;
	private bool _suppressPropagate;

	public bool IsClosed => _door is not null
		? _door.State == Sandbox.Mapping.Door.DoorState.Closed
		: !_animOpen;

	public bool IsOpen => _door is not null
		? _door.State == Sandbox.Mapping.Door.DoorState.Open
		: _animOpen;

	protected override void OnStart()
	{
		base.OnStart();
		GameObject.Tags.Add( "door" );
		DisplayName = "Door";
		Locked = StartLocked;
		_requiredItems.Clear();
		_requiredItems[ItemRequiredToOpen] = 1;

		_skinned = GetComponent<SkinnedModelRenderer>();
		SetupMappingDoor();
	}

	private void SetupMappingDoor()
	{
		if ( MoveDirType == 2 )
		{
			// AnimatingOnly door
			if ( InitialPosition >= 100f )
			{
				_animOpen = true;
				_skinned?.Set( "open", true );
			}
			return;
		}

		_door = GameObject.GetOrAddComponent<Sandbox.Mapping.Door>();
		_door.IsUsable = false; // Handled by DoorControllerComponent
		_door.IsLocked = false;
		_door.Speed = Speed > 0 ? Speed : 100f;
		_door.OpenAwayFromPlayer = OpenAwayFromPlayer;
		_door.StartOpen = InitialPosition >= 99f;
		_door.AutoClose = TimeBeforeReset >= 0;
		_door.AutoCloseDelay = TimeBeforeReset >= 0 ? TimeBeforeReset : -1f;

		if ( !string.IsNullOrEmpty( OpenSound ) ) _door.OpenSound = ResourceLibrary.Get<SoundEvent>( OpenSound );
		if ( !string.IsNullOrEmpty( FullyOpenSound ) ) _door.OpenFinishedSound = ResourceLibrary.Get<SoundEvent>( FullyOpenSound );
		if ( !string.IsNullOrEmpty( CloseSound ) ) _door.CloseSound = ResourceLibrary.Get<SoundEvent>( CloseSound );
		if ( !string.IsNullOrEmpty( FullyClosedSound ) ) _door.CloseFinishedSound = ResourceLibrary.Get<SoundEvent>( FullyClosedSound );
		if ( !string.IsNullOrEmpty( LockedSound ) ) _door.LockedSound = ResourceLibrary.Get<SoundEvent>( LockedSound );

		var mr = GetComponent<ModelRenderer>();
		var localBounds = mr?.Model?.Bounds.Size ?? new Vector3( 8, 64, 104 );

		if ( MoveDirType == 0 )
		{
			_door.Mode = Sandbox.Mapping.Door.DoorMode.Sliding;
			var dir = MoveDir.Forward;
			if ( !MoveDirIsLocal )
				dir = WorldRotation.Inverse * dir;
			var slideDist = MathF.Abs( localBounds.Dot( dir ) ) - Distance;
			_door.SlideOffset = dir * slideDist;
		}
		else
		{
			_door.Mode = Sandbox.Mapping.Door.DoorMode.Rotating;
			_door.TargetAngle = Distance;
		}
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		if ( !UseOpens || ObjectiveLocked ) return false;
		if ( !BypassItemRequirement && !user.Inventory.HasItem( ItemRequiredToOpen ) ) return false;
		return user.WorldPosition.Distance( WorldPosition ) < 100f;
	}

	public override bool OnUse( ColdOpenPlayer user )
	{
		if ( ObjectiveLocked ) return false;
		if ( TimeSinceUsed < 1f ) return false;

		if ( !BypassItemRequirement && !user.Inventory.HasItem( ItemRequiredToOpen ) )
		{
			if ( !string.IsNullOrEmpty( LockedSound ) )
				AudioDirector.PlayAt( LockedSound, WorldPosition );
			_skinned?.Set( "locked", true );
			TimeSinceUsed = 0;
			return false;
		}

		Toggle( user.GameObject );
		TimeSinceUsed = 0;
		return false;
	}

	public void Toggle( GameObject activator = null )
	{
		if ( _door is not null )
		{
			if ( _door.State == Sandbox.Mapping.Door.DoorState.Open || _door.State == Sandbox.Mapping.Door.DoorState.Opening )
			{
				Locked = true;
				_door.Close();
			}
			else
			{
				Locked = false;
				_door.Open( activator );
			}
		}
		else
		{
			_animOpen = !_animOpen;
			_skinned?.Set( "open", _animOpen );
			var snd = _animOpen ? OpenSound : CloseSound;
			if ( !string.IsNullOrEmpty( snd ) )
				AudioDirector.PlayAt( snd, WorldPosition );
		}

		PropagateToOtherDoors( activator );
	}

	private void PropagateToOtherDoors( GameObject activator )
	{
		if ( _suppressPropagate ) return;
		_suppressPropagate = true;

		foreach ( var other in Scene.GetAllComponents<DoorControllerComponent>() )
		{
			if ( other == this ) continue;
			bool matchSameName = !string.IsNullOrEmpty( GameObject.Name ) && other.GameObject.Name == GameObject.Name;
			bool matchTarget = !string.IsNullOrEmpty( OtherDoorsToOpen ) && other.GameObject.Name.Contains( OtherDoorsToOpen, StringComparison.OrdinalIgnoreCase );
			if ( matchSameName || matchTarget )
			{
				other._suppressPropagate = true;
				other.Toggle( activator );
				other._suppressPropagate = false;
			}
		}

		_suppressPropagate = false;
	}
}

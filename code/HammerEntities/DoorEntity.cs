using System.Threading.Tasks;
using Sandbox.ModelEditor.Nodes;
using Sandbox.Utility;

namespace GvarJam.HammerEntities;

/// <summary>
/// A basic door entity that can move or rotate. It can be a model or a mesh entity.
/// The door will rotate around the model's origin. For Hammer meshes the mesh origin can be set via the Pivot Tool.
/// </summary>
[Library( "ent_door_custom" )]
[HammerEntity, SupportsSolid]
[Model( Archetypes = ModelArchetype.animated_model )]
[DoorHelper( "movedir", "movedir_islocal", "movedir_type", "distance" )]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Title( "Door" ), Category( "Gameplay" ), Icon( "door_front" )]
public sealed partial class DoorEntity : KeyframeEntity, IUse
{
	/// <summary>
	/// The doors flags.
	/// </summary>
	[Flags]
	public enum Flags
	{
		UseOpens = 1,
		StartLocked = 2,
		//SpawnOpen = 4,
		//OneWay = 8,
		//Touch = 16,

		//StartUnbreakable = 524288,
	}

	/// <summary>
	/// The item that is required to open the door.
	/// </summary>
	[Property]
	public ItemType ItemRequiredToOpen { get; set; }

	/// <summary>
	/// Settings that are only applicable when the entity spawns
	/// </summary>
	[Property( "spawnsettings", Title = "Spawn Settings" )]
	public Flags SpawnSettings { get; set; } = Flags.UseOpens;

	/// <summary>
	/// The direction the door will move, when it opens.
	/// </summary>
	[Property( "movedir", Title = "Move Direction (Pitch Yaw Roll)" )]
	public Angles MoveDir { get; set; }

	/// <summary>
	/// If checked, the movement direction angle is in local space and should be rotated by the entity's angles after spawning.
	/// </summary>
	[Property( "movedir_islocal", Title = "Move Direction is Expressed in Local Space" )]
	public bool MoveDirIsLocal { get; set; } = true;

	/// <summary>
	/// Represents the way the door should move.
	/// </summary>
	public enum DoorMoveType
	{
		Moving,
		Rotating,
		AnimatingOnly
	}

	/// <summary>
	/// Movement type of the door.
	/// </summary>
	[Property( "movedir_type", Title = "Movement Type" )]
	public DoorMoveType MoveDirType { get; set; } = DoorMoveType.Moving;

	/// <summary>
	/// Moving door: The amount, in inches, of the door to leave sticking out of the wall it recedes into when pressed. Negative values make the door recede even further into the wall.
	/// Rotating door: The amount, in degrees, that the door should rotate when it's pressed.
	/// </summary>
	[Property]
	public float Distance { get; set; } = 0;

	/// <summary>
	/// How far the door should be open on spawn where 0% = closed and 100% = fully open.
	/// </summary>
	[Property( "initial_position" ), Category( "Spawn Settings" )]
	[MinMax( 0, 100 )]
	public float InitialPosition { get; set; } = 0;

	/// <summary>
	/// If checked, rotating doors will try to open away from the activator
	/// </summary>
	[Property( "open_away", Title = "Open Away From Player" ), Category( "Spawn Settings" )]
	public bool OpenAwayFromPlayer { get; set; } = false;

	/// <summary>
	/// The speed at which the door moves.
	/// </summary>
	[Property]
	public float Speed { get; set; } = 100;

	/// <summary>
	/// Amount of time, in seconds, after the door has opened before it closes automatically. If the value is set to -1, the door never closes itself.
	/// </summary>
	[Property( "close_delay", Title = "Auto Close Delay (-1 stay)" )]
	public float TimeBeforeReset { get; set; } = 4;

	/// <summary>
	/// If set, opening this door will open all doors with given entity name. You can also simply name all doors the same for this to work.
	/// </summary>
	[Property( "other_doors_to_open" )]
	public EntityTarget OtherDoorsToOpen { get; set; }

	/// <summary>
	/// If the door model supports break pieces and has prop_data with health, this option can be used to allow the door to break like a normal prop would.
	/// </summary>
	[Property] public bool Breakable { get; set; } = false;

	/// <summary>
	/// Sound to play when the door starts to open.
	/// </summary>
	[Property( "open_sound", Title = "Start Opening Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string OpenSound { get; set; } = "";

	/// <summary>
	/// Sound to play when the door reaches it's fully open position.
	/// </summary>
	[Property( "fully_open_sound", Title = "Fully Open Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string FullyOpenSound { get; set; } = "";

	/// <summary>
	/// Sound to play when the door starts to close.
	/// </summary>
	[Property( "close_sound", Title = "Start Closing Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string CloseSound { get; set; } = "";

	/// <summary>
	/// Sound to play when the door reaches it's fully closed position.
	/// </summary>
	[Property( "fully_closed_sound", Title = "Fully Closed Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string FullyClosedSound { get; set; } = "";

	/// <summary>
	/// Sound to play when the door is attempted to be opened, but is locked.
	/// </summary>
	[Property( "locked_sound", Title = "Locked Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string LockedSound { get; set; } = "";

	/// <summary>
	/// Sound to play while the door is moving. Typically this should be looping or very long.
	/// </summary>
	[Property( "moving_sound", Title = "Moving Sound" ), FGDType( "sound" ), Category( "Sounds" )]
	public string MovingSound { get; set; } = "";

	/// <summary>
	/// Used to override the open/close animation of moving and rotating doors. X axis (input, left to right) is the animation, Y axis (output, bottom to top) is how open the door is at that point in the animation.
	/// </summary>
	[Property( "open_ease", Title = "Ease Function" )]
	public FGDCurve OpenCurve { get; set; } = null!;

	/// <summary>
	/// Whether this door is locked or not.
	/// </summary>
	[Net]
	public bool Locked { get; set; } = true;

	[Net]
	public bool ObjectiveLocked { get; set; } = false;

	[Net, Predicted]
	public TimeSince TimeSinceUsed { get; set; }

	/// <summary>
	/// The easing function for both movement and rotation
	/// TODO: Expose to hammer in a nice way
	/// </summary>
	public Easing.Function Ease { get; set; } = Easing.EaseOut;

	Vector3 PositionA;
	Vector3 PositionB;
	Rotation RotationA;
	Rotation RotationB;
	Rotation RotationB_Normal;
	Rotation RotationB_Opposite;

	/// <summary>
	/// Represents a state that the door is in.
	/// </summary>
	public enum DoorState
	{
		Open,
		Closed,
		Opening,
		Closing
	}

	/// <summary>
	/// Which position the door is in.
	/// </summary>
	[Net]
	public DoorState State { get; protected set; } = DoorState.Open;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		// DoorMoveType.Moving
		{
			PositionA = LocalPosition;

			// Get the direction we want to move in
			var dir = MoveDir.Forward;

			// Open position is the size of the bbox in the direction minus the lip size
			var boundSize = CollisionBounds.Size;

			PositionB = PositionA + dir * (MathF.Abs( boundSize.Dot( dir ) ) - Distance);

			if ( MoveDirIsLocal )
			{
				var dir_world = Transform.NormalToWorld( dir );
				PositionB = PositionA + dir_world * (MathF.Abs( boundSize.Dot( dir ) ) - Distance);
			}
		}

		// DoorMoveType.Rotating
		{
			RotationA = LocalRotation;

			var axis = Rotation.From( MoveDir ).Up;
			if ( !MoveDirIsLocal ) axis = Transform.NormalToLocal( axis );

			RotationB_Opposite = RotationA.RotateAroundAxis( axis, -Distance );
			RotationB_Normal = RotationA.RotateAroundAxis( axis, Distance );
			RotationB = RotationB_Normal;
		}

		State = DoorState.Closed;
		Locked = SpawnSettings.HasFlag( Flags.StartLocked );

		if ( InitialPosition > 0 )
		{
			SetPosition( InitialPosition / 100.0f );
		}

		if ( OpenCurve != null )
		{
			Ease = delegate ( float x ) { return OpenCurve.GetNormalized( x ); };
		}
	}

	/// <inheritdoc/>
	protected override void OnDestroy()
	{
		if ( MoveSoundInstance.HasValue )
		{
			MoveSoundInstance.Value.Stop();
			MoveSoundInstance = null;
		}

		base.OnDestroy();
	}

	/// <summary>
	/// Sets the door's position to given percentage. The expected input range is 0..1
	/// </summary>
	[Input]
	private void SetPosition( float progress )
	{
		if ( MoveDirType == DoorMoveType.Moving ) { LocalPosition = PositionA.LerpTo( PositionB, progress ); }
		else if ( MoveDirType == DoorMoveType.Rotating ) { LocalRotation = Rotation.Lerp( RotationA, RotationB, progress ); }
		else if ( MoveDirType == DoorMoveType.AnimatingOnly )
		{
			SetAnimParameter( "initial_position", progress );
			SetAnimParameter( "update_position", true );

			// We have to be considered closed, as the initial_position is meant to affect the opening animation
			UpdateAnimGraph( false );
			State = DoorState.Closed;
			// TODO: Should this call OpenOtherDoors?
		}
		else { Log.Warning( $"{this}: Unknown door move type {MoveDirType}!" ); }

		if ( progress >= 1.0f ) State = DoorState.Open;
	}

	/// <summary>
	/// Updates the open state on the doors animgraph.
	/// </summary>
	/// <param name="open">The open state of the door.</param>
	private void UpdateAnimGraph( bool open )
	{
		SetAnimParameter( "open", open );
	}

	/// <inheritdoc/>
	protected override void OnAnimGraphCreated()
	{
		base.OnAnimGraphCreated();

		// Anim graph doesnt exist in Spawn()
		if ( State == DoorState.Open )
		{
			UpdateAnimGraph( true );
		}

		if ( InitialPosition > 0 )
		{
			SetAnimParameter( "initial_position", InitialPosition / 100.0f );
		}
	}

	/// <inheritdoc/>
	public bool IsUsable( Entity user ) => SpawnSettings.HasFlag( Flags.UseOpens );

	/// <summary>
	/// Fired when a player tries to open/close this door with +use, but it's locked
	/// </summary>
	protected Output OnLockedUse { get; set; }

	/// <inheritdoc/>
	public bool OnUse( Entity user )
	{
		if ( ObjectiveLocked )
			return false;

		if ( TimeSinceUsed < 1 )
			return false;

		if ( Locked && !(user as Pawn)!.HasItem(ItemRequiredToOpen) )
		{
			PlaySound( LockedSound );
			SetAnimParameter( "locked", true );
			OnLockedUse.Fire( this );

			TimeSinceUsed = 0;

			return false;
		}

		if ( (user as Pawn)!.HasItem( ItemRequiredToOpen ) )
		{
			Toggle( user );
		}

		TimeSinceUsed = 0;

		return false;
	}

	#region Breakable
	/// <inheritdoc/>
	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		// When a model is reloaded, all entities get set to NULL model first
		if ( model == null || model.IsError ) return;

		if ( !Game.IsServer ) return;

		if ( model.TryGetData( out Sandbox.DoorEntity.ModelDoorSounds sounds ) )
		{
			if ( string.IsNullOrEmpty( MovingSound ) ) MovingSound = sounds.MovingSound;
			if ( string.IsNullOrEmpty( CloseSound ) ) CloseSound = sounds.CloseSound;
			if ( string.IsNullOrEmpty( FullyClosedSound ) ) FullyClosedSound = sounds.FullyClosedSound;
			if ( string.IsNullOrEmpty( OpenSound ) ) OpenSound = sounds.OpenSound;
			if ( string.IsNullOrEmpty( FullyOpenSound ) ) FullyOpenSound = sounds.FullyOpenSound;
			if ( string.IsNullOrEmpty( LockedSound ) ) LockedSound = sounds.LockedSound;
		}

		if ( model.TryGetData( out ModelPropData propInfo ) )
		{
			Health = propInfo.Health;
		}

		// If health is unset, set it to -1 - which means it cannot be destroyed
		if ( Health <= 0 ) Health = -1;
	}

	DamageInfo LastDamage;

	/// <summary>
	/// Fired when the entity gets damaged, even if it is unbreakable.
	/// </summary>
	protected Output OnDamaged { get; set; }

	/// <inheritdoc/>
	public override void TakeDamage( DamageInfo info )
	{
		// The door was damaged, even if its unbreakable, we still want to fire it
		// TODO: Add damage type as argument? Or should it be the new health value?
		OnDamaged.Fire( this );

		if ( !Breakable ) return;

		base.TakeDamage( info );

		LastDamage = info;
	}

	/// <summary>
	/// Fired when the entity gets destroyed.
	/// </summary>
	protected Output OnBreak { get; set; }

	/// <inheritdoc/>
	public override void OnKilled()
	{
		if ( LifeState != LifeState.Alive )
			return;

		var result = new Breakables.Result();
		result.CopyParamsFrom( LastDamage );
		Breakables.Break( this, result );

		OnBreak.Fire( LastDamage.Attacker );

		base.OnKilled();
	}

	/// <summary>
	/// Causes this prop to break, regardless if it is actually breakable or not. (i.e. ignores health and whether the model has gibs)
	/// </summary>
	[Input]
	public void Break()
	{
		OnKilled();
		LifeState = LifeState.Dead;
		Delete();
	}

	#endregion

	/// <summary>
	/// Toggle the open state of the door. Obeys locked state.
	/// </summary>
	[Input]
	public void Toggle( Entity? activator = null )
	{
		if ( State == DoorState.Open || State == DoorState.Opening )
		{
			Close( activator );
			Locked = true;
		}
		else if ( State == DoorState.Closed || State == DoorState.Closing )
		{
			Locked = false;
			Open( activator );
		}	
	}

	/// <summary>
	/// Open the door. Obeys locked state.
	/// </summary>
	[Input]
	public void Open( Entity? activator = null )
	{
		if ( Locked )
		{
			PlaySound( LockedSound );
			SetAnimParameter( "locked", true );
			return;
		}

		if ( State == DoorState.Closed )
		{
			PlaySound( OpenSound );
		}

		if ( State == DoorState.Closed || State == DoorState.Closing ) State = DoorState.Opening;

		if ( activator != null && MoveDirType == DoorMoveType.Rotating && OpenAwayFromPlayer && State != DoorState.Open )
		{
			// TODO: In this case the door could be moving faster than given speed if we are trying to open the door while it is closing from the opposite side
			var axis = Rotation.From( MoveDir ).Up;
			if ( !MoveDirIsLocal ) axis = Transform.NormalToLocal( axis );

			// Generate the correct "inward" direction for the door since we can't assume RotationA.Forward is it
			// TODO: This does not handle non UP axis doors!
			var Dir = (WorldSpaceBounds.Center.WithZ( Position.z ) - Position).Normal;
			var Pos1 = Position + Rotation.FromAxis( Dir, 0 ).RotateAroundAxis( axis, Distance ) * Dir * 24.0f;
			var Pos2 = Position + Rotation.FromAxis( Dir, 0 ).RotateAroundAxis( axis, -Distance ) * Dir * 24.0f;

			var PlyPos = activator.Position;
			if ( PlyPos.Distance( Pos2 ) < PlyPos.Distance( Pos1 ) )
			{
				RotationB = RotationB_Normal;
			}
			else
			{
				RotationB = RotationB_Opposite;
			}
		}

		UpdateAnimGraph( true );

		UpdateState();

		OpenOtherDoors( true, activator );
	}

	/// <summary>
	/// Close the door. Obeys locked state.
	/// </summary>
	[Input]
	public void Close( Entity? activator = null )
	{
		if ( Locked )
		{
			PlaySound( LockedSound );

			SetAnimParameter( "locked", true );
			return;
		}

		if ( State == DoorState.Open )
		{
			PlaySound( CloseSound );
		}

		if ( State == DoorState.Open || State == DoorState.Opening ) State = DoorState.Closing;

		UpdateAnimGraph( false );
		UpdateState();

		OpenOtherDoors( false, activator );
	}

	/// <summary>
	/// Locks the door so it cannot be opened or closed.
	/// </summary>
	[Input]
	public void Lock()
	{
		Locked = true;
	}

	/// <summary>
	/// Unlocks the door.
	/// </summary>
	[Input]
	public void Unlock()
	{
		Locked = false;
	}

	/// <summary>
	/// Fired when the door starts to open. This can be called multiple times during a single "door opening"
	/// </summary>
	protected Output OnOpen { get; set; }

	/// <summary>
	/// Fired when the door starts to close. This can be called multiple times during a single "door closing"
	/// </summary>
	protected Output OnClose { get; set; }

	/// <summary>
	/// Called when the door fully opens.
	/// </summary>
	protected Output OnFullyOpen { get; set; }

	/// <summary>
	/// Called when the door fully closes.
	/// </summary>
	protected Output OnFullyClosed { get; set; }

	internal bool ShouldPropagateState = true;
	private void OpenOtherDoors( bool open, Entity? activator )
	{
		if ( !ShouldPropagateState ) return;

		List<Entity> ents = new();

		if ( !string.IsNullOrEmpty( Name ) ) ents.AddRange( FindAllByName( Name ) );
		if ( OtherDoorsToOpen.TryGetTargets( out Entity[] doors ) ) ents.AddRange( doors );

		foreach ( var ent in ents )
		{
			if ( ent == this || ent is not DoorEntity ) continue;

			DoorEntity door = (DoorEntity)ent;

			door.ShouldPropagateState = false;
			if ( open )
			{
				door.Open( activator );
			}
			else
			{
				door.Close( activator );
			}
			door.ShouldPropagateState = true;
		}
	}

	private void UpdateState()
	{
		bool open = (State == DoorState.Opening) || (State == DoorState.Open);

		_ = DoMove( open );
	}

	int movement = 0;
	Sound? MoveSoundInstance = null;
	bool AnimGraphFinished = false;
	async Task DoMove( bool state )
	{
		if ( !MoveSoundInstance.HasValue && !string.IsNullOrEmpty( MovingSound ) )
		{
			MoveSoundInstance = PlaySound( MovingSound );
		}

		var moveId = ++movement;

		if ( State == DoorState.Opening )
		{
			_ = OnOpen.Fire( this );
		}
		else if ( State == DoorState.Closing )
		{
			_ = OnClose.Fire( this );
		}

		if ( MoveDirType == DoorMoveType.Moving )
		{
			var position = state ? PositionB : PositionA;
			var distance = Vector3.DistanceBetween( LocalPosition, position );
			var timeToTake = distance / Math.Max( Speed, 0.1f );

			var success = await LocalKeyframeTo( position, timeToTake, Ease );
			if ( !success )
				return;
		}
		else if ( MoveDirType == DoorMoveType.Rotating )
		{
			var target = state ? RotationB : RotationA;

			Rotation diff = LocalRotation * target.Inverse;
			var timeToTake = diff.Angle() / Math.Max( Speed, 0.1f );

			var success = await LocalRotateKeyframeTo( target, timeToTake, Ease );
			if ( !success )
				return;
		}
		else if ( MoveDirType == DoorMoveType.AnimatingOnly )
		{
			AnimGraphFinished = false;
			while ( !AnimGraphFinished )
			{
				await Task.Delay( 100 );
			}
		}
		else { Log.Warning( $"{this}: Unknown door move type {MoveDirType}!" ); }

		if ( moveId != movement || !this.IsValid() )
			return;

		if ( State == DoorState.Opening )
		{
			_ = OnFullyOpen.Fire( this );
			State = DoorState.Open;
			PlaySound( FullyOpenSound );
		}
		else if ( State == DoorState.Closing )
		{
			_ = OnFullyClosed.Fire( this );
			State = DoorState.Closed;
			PlaySound( FullyClosedSound );
		}

		if ( MoveSoundInstance.HasValue )
		{
			MoveSoundInstance.Value.Stop();
			MoveSoundInstance = null;
		}

		if ( state && TimeBeforeReset >= 0 )
		{
			await Task.DelaySeconds( TimeBeforeReset );

			if ( moveId != movement || !this.IsValid() )
				return;

			Toggle();
		}
	}

	/// <inheritdoc/>
	protected override void OnAnimGraphTag( string tag, AnimGraphTagEvent fireMode )
	{
		if ( tag == "AnimationFinished" && fireMode != AnimGraphTagEvent.End )
		{
			AnimGraphFinished = true;
		}
	}
}

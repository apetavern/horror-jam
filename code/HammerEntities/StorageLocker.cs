using System.Numerics;

namespace GvarJam.HammerEntities;

/// <summary>
/// Represents a storage locker that can contain items.
/// </summary>
[Category( "Environment" )]
[Library( "ent_storagelocker" )]
[HammerEntity]
[EditorModel( "models/scifilocker/scifilocker.vmdl" )]
public sealed partial class StorageLocker : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 1.1f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => false;
	/// <inheritdoc/>
	protected override bool ResetOnUse => false;

	/// <inheritdoc/>
	public override IReadOnlyDictionary<ItemType, int> RequiredItems => requiredItems;
	/// <summary>
	/// The required items to use the storage locker.
	/// </summary>
	private readonly Dictionary<ItemType, int> requiredItems = new();

	/// <summary>
	/// Whether or not this locker is locked.
	/// </summary>
	[Net, Property]
	public bool IsLocked { get; set; } = true;

	/// <summary>
	/// Whether or not this locker will randomly choose an item at runtime.
	/// </summary>
	[Net, Property]
	public bool RandomItem { get; set; } = true;

	/// <summary>
	/// The class of the item to spawn if <see cref="RandomItem"/> is false.
	/// </summary>
	[Net, Property]
	public string ItemClass { get; set; } = string.Empty;

	/// <summary>
	/// Whether or not the locker has been opened.
	/// </summary>
	[Net, Predicted]
	public bool Opened { get; set; }

	/// <summary>
	/// The starting transform of the locker door.
	/// </summary>
	private Transform StartDoorTransform;

	/// <summary>
	/// The door of the locker.
	/// </summary>
	[Net, Predicted]
	ModelEntity Door { get; set; } = null!;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Storage Locker";

		SetModel( "models/scifilocker/scifilocker.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );

		Door = new( "models/scifilocker/scifilocker_door.vmdl" )
		{
			Position = GetAttachment( "doorhinge" )!.Value.Position,
			Rotation = Rotation
		};
		Door.Tags.Add( "camignore" );
		Door.SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Door.SetParent( this );

		StartDoorTransform = Door.Transform;//GetBoneTransform( GetBoneIndex( "Hinge" ) );

		if ( IsLocked )
			requiredItems.Add( ItemType.JanitorKey, 1 );

		ModelEntity entity;
		if ( RandomItem )
			entity = PickRandomItem();
		else
			entity = TypeLibrary.Create<ModelEntity>( ItemClass );

		if ( entity is null )
			return;

		entity.SetupPhysicsFromModel( PhysicsMotionType.Static );

		// TODO: Attachment point locker for items?
		entity.Position = Position + (Vector3.Up * 2) + (Rotation.Left.Normal * 10);
		entity.Rotation = Rotation.FromYaw( 90 );

	}

	/// <inheritdoc/>
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		StartDoorTransform = GetBoneTransform( GetBoneIndex( "Hinge" ) );

		if ( IsLocked )
			requiredItems.Add( ItemType.JanitorKey, 1 );
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (0.6f, UnlockLockerDoor) );
		Actions.Add( (0.5f, OpenLockerDoor) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		Opened = true;
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		return !Opened && base.IsUsable( user );
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		base.Reset();

		if ( IsServer )
			Door.Transform = StartDoorTransform; //SetBone( GetBoneIndex( "Hinge" ), StartDoorTransform );
		Opened = false;
	}

	/// <summary>
	/// The first animation, unlocks the locker door.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool UnlockLockerDoor( Entity user, bool firstTime, float timeInAnim )
	{
		if ( !IsLocked )
			return true;

		if ( IsServer && timeInAnim >= 0.55 )
			IsLocked = false;

		TraceResult floortrace = Trace.Ray( GetAttachment( "openstandpos" )!.Value.Position, GetAttachment( "openstandpos" )!.Value.Position - Vector3.Up * 100f )
			.WithoutTags( "camignore" )
			.Ignore( user )
			.Run();

		user.Position = floortrace.EndPosition + Rotation.Right * 5f;
		user.Rotation = Rotation.LookAt( (Position - user.Position).WithZ( 0 ), Vector3.Up );

		(user as Pawn)!.SetAnimParameter( "b_IKleft", true );
		(user as Pawn)!.SetAnimParameter( "left_hand_ik.position", Door.GetAttachment( "handle" )!.Value.Position );
		(user as Pawn)!.SetAnimParameter( "left_hand_ik.rotation", Rotation * new Angles( 0, -70, 90 ).ToRotation() );
		Sound.FromEntity( "locker_open", this );
		return false;
	}

	/// <summary>
	/// The second animation, opens the locker door.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for.</param>
	/// <returns>Whether or not to skip this animation.</returns>
	private bool OpenLockerDoor( Entity user, bool firstTime, float timeInAnim )
	{
		if ( Opened )
		{
			return true;
		}

		if ( IsServer )
		{
			(user as Pawn)!.SetAnimParameter( "b_IKleft", true );
			(user as Pawn)!.SetAnimParameter( "left_hand_ik.position", Door.GetAttachment( "handle" )!.Value.Position );
			(user as Pawn)!.SetAnimParameter( "left_hand_ik.rotation", Rotation * new Angles( 0, -80, 90 ).ToRotation() );
			var targetTransform = Transform;
			targetTransform.Rotation *= (Rotation)Quaternion.CreateFromAxisAngle( new Vector3( 0, 0, 1 ), -90 );

			TraceResult floortrace = Trace.Ray( GetAttachment( "openstandpos" )!.Value.Position, GetAttachment( "openstandpos" )!.Value.Position - Vector3.Up * 100f )
			.WithoutTags( "camignore" )
			.Ignore( user )
			.Run();

			user.Position = floortrace.EndPosition + Rotation.Right * 5f;
			user.Rotation = Rotation.LookAt( (Position - user.Position).WithZ( 0 ), Vector3.Up );

			Door.Owner = user;

			Door.Rotation = Rotation.Lerp( Door.Rotation, targetTransform.Rotation, timeInAnim / 0.5f, false );
			if ( timeInAnim > 0.45f )
			{
				(user as Pawn)!.SetAnimParameter( "b_IKleft", false );
			}
		}

		return false;
	}

	/// <summary>
	/// Makes a random item to spawn in the locker.
	/// </summary>
	/// <returns></returns>
	private ModelEntity PickRandomItem()
	{
		return new LampBattery();
	}
}


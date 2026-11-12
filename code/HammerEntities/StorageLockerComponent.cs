namespace GvarJam.HammerEntities;

public sealed class StorageLockerComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "StorageLocker";
	public override float TimeToUse => 1.1f;
	protected override bool DeleteOnUse => false;
	protected override bool ResetOnUse => false;

	[Property, Sync] public bool IsLocked { get; set; } = true;
	[Property, Sync] public bool RandomItem { get; set; } = true;
	[Property, Sync] public string ItemClass { get; set; } = string.Empty;
	[Sync] public bool Opened { get; set; }

	private Transform _startDoorLocalTransform;
	private GameObject _doorGo;
	private ModelRenderer _bodyRenderer;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Storage Locker";

		var bodyModel = Model.Load( "models/scifilocker/scifilocker.vmdl" );
		_bodyRenderer = GameObject.GetOrAddComponent<ModelRenderer>();
		_bodyRenderer.Model = bodyModel;
		_bodyRenderer.CreateAttachments = true;

		var bodyCol = GameObject.GetOrAddComponent<ModelCollider>();
		bodyCol.Model = bodyModel;

		var hingeObj = _bodyRenderer.GetAttachmentObject( "doorhinge" );
		_doorGo = new GameObject( true, "LockerDoor" );
		_doorGo.SetParent( GameObject, false );
		_doorGo.WorldPosition = hingeObj?.WorldPosition ?? WorldPosition;
		_doorGo.WorldRotation = WorldRotation;
		_doorGo.Tags.Add( "camignore" );

		var doorModel = Model.Load( "models/scifilocker/scifilocker_door.vmdl" );
		var doorMr = _doorGo.AddComponent<ModelRenderer>();
		doorMr.Model = doorModel;
		doorMr.CreateAttachments = true;
		var doorCol = _doorGo.AddComponent<ModelCollider>();
		doorCol.Model = doorModel;

		_startDoorLocalTransform = _doorGo.LocalTransform;

		_requiredItems.Clear();
		if ( IsLocked )
			_requiredItems[ItemType.JanitorKey] = 1;

		SpawnContents();
	}

	private void SpawnContents()
	{
		var itemGo = new GameObject( true, "LockerItem" );
		itemGo.WorldPosition = WorldPosition + (Vector3.Up * 2f) + (WorldRotation.Left.Normal * 10f);
		itemGo.WorldRotation = WorldRotation * Rotation.FromYaw( 90f );

		if ( RandomItem || string.IsNullOrEmpty( ItemClass ) || ItemClass.Contains( "battery", StringComparison.OrdinalIgnoreCase ) )
		{
			itemGo.AddComponent<LampBatteryComponent>();
		}
		else if ( ItemClass.Contains( "item", StringComparison.OrdinalIgnoreCase ) )
		{
			itemGo.AddComponent<InventoryItemComponent>();
		}
		else
		{
			itemGo.AddComponent<LampBatteryComponent>();
		}
	}

	protected override void CreateActions()
	{
		Actions.Add( (0.6f, UnlockLockerDoor) );
		Actions.Add( (0.5f, OpenLockerDoor) );
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		Opened = true;
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return !Opened && base.IsUsable( user );
	}

	public override void ResetInteraction()
	{
		base.ResetInteraction();
		if ( _doorGo.IsValid() )
			_doorGo.LocalTransform = _startDoorLocalTransform;
		Opened = false;
	}

	private bool UnlockLockerDoor( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( !IsLocked ) return true;

		if ( timeInAnim >= 0.55f )
		{
			IsLocked = false;
			_requiredItems.Clear();
		}

		SnapToStandPos( user );
		var handleObj = _doorGo?.GetComponent<ModelRenderer>()?.GetAttachmentObject( "handle" );
		if ( user.BodyRenderer.IsValid() )
		{
			user.BodyRenderer.Set( "b_IKleft", true );
			if ( handleObj is not null )
			{
				user.BodyRenderer.Set( "left_hand_ik.position", handleObj.WorldPosition );
				user.BodyRenderer.Set( "left_hand_ik.rotation", WorldRotation * new Angles( 0, -70, 90 ).ToRotation() );
			}
		}

		if ( firstTime )
			AudioDirector.PlayAt( "locker_open", WorldPosition );

		return false;
	}

	private bool OpenLockerDoor( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( Opened ) return true;

		var handleObj = _doorGo?.GetComponent<ModelRenderer>()?.GetAttachmentObject( "handle" );
		if ( user.BodyRenderer.IsValid() )
		{
			user.BodyRenderer.Set( "b_IKleft", true );
			if ( handleObj is not null )
			{
				user.BodyRenderer.Set( "left_hand_ik.position", handleObj.WorldPosition );
				user.BodyRenderer.Set( "left_hand_ik.rotation", WorldRotation * new Angles( 0, -80, 90 ).ToRotation() );
			}
		}

		SnapToStandPos( user );

		if ( _doorGo.IsValid() )
		{
			var targetRot = WorldRotation * Rotation.FromAxis( Vector3.Up, -90f );
			_doorGo.WorldRotation = Rotation.Lerp( _doorGo.WorldRotation, targetRot, (timeInAnim / 0.5f).Clamp( 0f, 1f ) );
		}

		if ( timeInAnim > 0.45f && user.BodyRenderer.IsValid() )
			user.BodyRenderer.Set( "b_IKleft", false );

		return false;
	}

	private void SnapToStandPos( ColdOpenPlayer user )
	{
		var standPos = _bodyRenderer?.GetAttachmentObject( "openstandpos" )?.WorldPosition ?? (WorldPosition + WorldRotation.Forward * 36f);
		var tr = Scene.Trace.Ray( standPos, standPos - Vector3.Up * 100f )
			.WithoutTags( "camignore", "player", "trigger" )
			.Run();

		user.WorldPosition = tr.EndPosition + WorldRotation.Right * 5f;
		var look = (WorldPosition - user.WorldPosition).WithZ( 0 );
		if ( !look.IsNearZeroLength )
			user.WorldRotation = Rotation.LookAt( look.Normal, Vector3.Up );
	}
}

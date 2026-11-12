namespace GvarJam.Interactions;

public sealed class LampBatteryComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "LampBattery";
	public override float TimeToUse => 2.6f;
	protected override bool DeleteOnUse => true;

	[Property, Sync] public float BatteryPower { get; set; } = 100f;

	private bool _droppedBattery;
	private bool _pickedUp;
	private bool _pushed;

	protected override void OnStart()
	{
		base.OnStart();
		DisplayName = "Battery";

		var model = Model.Load( "models/items/battery/battery.vmdl" );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;

		if ( GameObject.Name != "DroppedBattery" )
			Scene?.GetSystem<ColdOpenGame>()?.RegisterBatterySpawn( this );
	}

	protected override void CreateActions()
	{
		Actions.Add( (0.8f, PullBattery) );
		Actions.Add( (0.6f, PickupItem) );
		Actions.Add( (0.8f, PushBattery) );
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		user.Lamp.BatteryInserted = true;
		user.Lamp.LampPower = BatteryPower;
	}

	public override bool IsUsable( ColdOpenPlayer user )
	{
		return user.Lamp.HasHelmet && base.IsUsable( user );
	}

	public override void ResetInteraction()
	{
		_droppedBattery = false;
		_pushed = false;

		if ( _pickedUp )
		{
			GameObject.SetParent( null, true );
			var tr = Scene.Trace.Ray( WorldPosition, WorldPosition - Vector3.Up * 100f )
				.WithoutTags( "player", "trigger" )
				.Run();
			WorldPosition = tr.EndPosition;
			WorldRotation = Rotation.Identity;
			_pickedUp = false;
		}

		base.ResetInteraction();
	}

	private bool PullBattery( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( firstTime )
		{
			var dir = (WorldPosition - user.WorldPosition).WithZ( 0 );
			if ( !dir.IsNearZeroLength )
				user.WorldRotation = Rotation.LookAt( dir.Normal, Vector3.Up );
		}

		if ( !user.Lamp.BatteryInserted )
			return true;

		if ( timeInAnim >= 0.6f && !_droppedBattery )
			DropOldBattery( user );

		user.BodyRenderer?.Set( "pullbattery", true );
		return false;
	}

	private void DropOldBattery( ColdOpenPlayer user )
	{
		if ( !user.Lamp.BatteryInserted )
			return;

		_droppedBattery = true;

		var boneTx = user.BodyRenderer?.GetBoneObject( "hold_L" )?.WorldTransform ?? user.WorldTransform;
		var go = new GameObject( true, "DroppedBattery" );
		go.WorldTransform = boneTx.WithScale( 0.6f );
		go.Tags.Add( "camignore" );

		var model = Model.Load( "models/items/battery/battery.vmdl" );
		var mr = go.AddComponent<ModelRenderer>();
		mr.Model = model;
		mr.Tint = new Color( 1f - user.Lamp.BatteryPercentage, user.Lamp.BatteryPercentage, 0f );

		var col = go.AddComponent<ModelCollider>();
		col.Model = model;

		var rb = go.AddComponent<Rigidbody>();
		rb.Velocity = user.WorldRotation.Left * 100f;

		if ( user.Lamp.LampPower > 0f )
		{
			var pickup = go.AddComponent<LampBatteryComponent>();
			pickup.BatteryPower = user.Lamp.LampPower;
		}
		else
		{
			_ = go.DeleteAfterSecondsAndNotVisible( 2.5f );
		}

		user.Lamp.BatteryInserted = false;
		AudioDirector.PlayAt( "battery_replace_1", user.WorldPosition );
	}

	private bool PickupItem( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( timeInAnim >= 0.55f && !_pickedUp )
		{
			_pickedUp = true;
			GameObject.Tags.Add( "camignore" );
			GameObject.SetParent( user.GameObject, true );
			WorldScale = 0.6f;
		}

		user.BodyRenderer?.Set( "grabitem", true );
		return false;
	}

	private bool PushBattery( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( timeInAnim >= 0.799f && !_pushed )
		{
			_pushed = true;
			AudioDirector.PlayAt( "battery_replace_2", user.WorldPosition );
		}

		user.BodyRenderer?.Set( "pushbattery", true );
		return false;
	}
}

namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Category( "Interactables" )]
[HammerEntity]
[EditorModel( "models/items/battery/battery.vmdl" )]
public partial class LampBatteryItem : DelayedUseItem
{
	public override float TimeToUse => 1;
	protected override bool DeleteOnUse => true;

	public override void Spawn()
	{
		base.Spawn();

		Name = "Battery";

		SetModel( "models/items/battery/battery.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	protected override void OnUsed( Entity user )
	{
		if ( user is not Pawn pawn )
			return;

		pawn.InsertNewLampBattery();

		Log.Info( $"Battery level replenished for {user.Name}" );
	}
}

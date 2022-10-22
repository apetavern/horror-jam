using System.Collections.Generic;

namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Category( "Interactables" )]
[HammerEntity]
[EditorModel( "models/items/battery/battery.vmdl" )]
public partial class LampBatteryItem : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 2.6f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => true;
	/// <inheritdoc/>
	protected override List<(float, Action<Entity>)> Actions => actions;
	/// <summary>
	/// The list of animations to play while interacting with the item.
	/// </summary>
	private readonly List<(float, Action<Entity>)> actions = new()
	{
		(0.4f, PullBattery),
		(Time.Delta, DropOldBattery),
		(0.5f, PickupItem),
		(0.8f, PushBattery)
	};

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Battery";

		SetModel( "models/items/battery/battery.vmdl" );

		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		if ( user is not Pawn pawn )
			return;

		pawn.InsertNewLampBattery();

		Log.Info( $"Battery level replenished for {user.Name}" );
	}

	/// <summary>
	/// The first animation, pulls the battery out of the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private static void PullBattery( Entity user )
	{
		(user as Pawn)!.SetAnimParameter( "pullbattery", true );
	}

	/// <summary>
	/// The first animation, pulls the battery out of the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private static void DropOldBattery( Entity user )
	{
		ModelEntity modl = new ModelEntity( "models/items/battery/battery.vmdl" );
		modl.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		modl.Transform = (user as AnimatedEntity).GetBoneTransform( "hold_L" ).WithScale(0.6f);
		modl.RenderColor = Color.Red;
		modl.DeleteAsync( 2.5f );
	}

	/// <summary>
	/// The second animation, picks up the battery item.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private static void PickupItem( Entity user )
	{

		(user as Pawn)!.SetAnimParameter( "grabitem", true );
	}

	/// <summary>
	/// The third animation, pushes the new battery into the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private static void PushBattery( Entity user )
	{
		(user as Pawn)!.SetAnimParameter( "pushbattery", true );
	}
}

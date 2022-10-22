using GvarJam.Utility;

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

	/// <summary>
	/// Whether or not the old battery in the users helmet was dropped.
	/// </summary>
	private bool droppedBattery;
	/// <summary>
	/// Whether or not the battery was picked up.
	/// </summary>
	private bool pickedUp;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = "Battery";

		SetModel( "models/items/battery/battery.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (0.8f, PullBattery) );
		Actions.Add( (0.6f, PickupItem) );
		Actions.Add( (0.8f, PushBattery) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		if ( user is not Pawn pawn )
			return;

		pawn.BatteryInserted = true;
		if ( IsServer )
			Delete();
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		droppedBattery = false;

		if ( pickedUp )
		{
			SetParent( null );
			Position = Trace.Ray( Position, Position - Vector3.Up * 100f )
				.WorldOnly()
				.Run().EndPosition;
			Rotation = Rotation.Identity;
			pickedUp = false;
		}

		base.Reset();
	}

	/// <summary>
	/// The first animation, pulls the battery out of the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for,</param>
	private bool PullBattery( Entity user, bool firstTime, float timeInAnim )
	{
		var pawn = (user as Pawn)!;
		if ( !pawn.BatteryInserted )
			return true;

		if ( IsServer && timeInAnim >= 0.52 && !droppedBattery )
		{
			DropOldBattery( user );
			Sound.FromEntity( "replace_battery", user );
		}

		pawn.SetAnimParameter( "pullbattery", true );
		return false;
	}

	/// <summary>
	/// Drops the old battery inside the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private void DropOldBattery( Entity user )
	{
		droppedBattery = true;

		var pawn = (user as Pawn)!;
		var modl = new ModelEntity( "models/items/battery/battery.vmdl" )
		{
			Transform = pawn.GetBoneTransform( "hold_L" ).WithScale( 0.6f ),
			Velocity = user.Rotation.Left * 100f,
			RenderColor = new Color( 1 - pawn.BatteryPercentage, pawn.BatteryPercentage, 0 )
		};
		modl.Tags.Add( "camignore" );
		modl.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		pawn.BatteryInserted = false;

		_ = modl.DeleteAfterSecondsAndNotVisible( 2.5f );
	}

	/// <summary>
	/// The second animation, picks up the battery item.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for,</param>
	private bool PickupItem( Entity user, bool firstTime, float timeInAnim )
	{
		if ( IsServer && timeInAnim >= 0.55 && !pickedUp )
			PickupBattery( user );

		(user as Pawn)!.SetAnimParameter( "grabitem", true );
		return false;
	}

	/// <summary>
	/// Picks up the new battery.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private void PickupBattery(Entity user )
	{
		pickedUp = true;

		Tags.Add( "camignore" );
		SetParent( user, true );
		Scale = 0.6f;
	}

	/// <summary>
	/// The third animation, pushes the new battery into the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for,</param>
	private bool PushBattery( Entity user, bool firstTime, float timeInAnim )
	{
		(user as Pawn)!.SetAnimParameter( "pushbattery", true );
		return false;
	}
}

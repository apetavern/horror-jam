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

		pawn.InsertNewLampBattery();

		Log.Info( $"Battery level replenished for {user.Name}" );
	}

	/// <summary>
	/// The first animation, pulls the battery out of the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	private void PullBattery( Entity user, bool firstTime )
	{
		if (IsServer && firstTime )
		{
			DropOldBattery( user );
		}
		(user as Pawn)!.SetAnimParameter( "pullbattery", true );
	}

	private async void DropOldBattery( Entity user )
	{
		await Task.DelaySeconds( 0.52f );
		ModelEntity modl = new ModelEntity( "models/items/battery/battery.vmdl" );
		modl.Tags.Add( "camignore" );
		modl.SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		modl.Transform = (user as AnimatedEntity).GetBoneTransform( "hold_L" ).WithScale(0.6f);
		modl.Velocity = user.Rotation.Left * 100f;
		modl.RenderColor = Color.Red;
		modl.DeleteAsync( 2.5f );
	}

	/// <summary>
	/// The second animation, picks up the battery item.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	private void PickupItem( Entity user, bool firstTime )
	{
		if ( IsServer && firstTime )
		{
			DelayedPickup( user );
		}

		(user as Pawn)!.SetAnimParameter( "grabitem", true );
	}

	private async void DelayedPickup(Entity user )
	{
		await Task.DelaySeconds( 0.8f );
		Tags.Add( "camignore" );
		
		SetParent( user, true );
		Scale = 0.6f;
	}

	/// <summary>
	/// The third animation, pushes the new battery into the helmet.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	private void PushBattery( Entity user, bool firstTime )
	{
		if ( IsServer && firstTime )
		{
			DisableRender();
		}
		(user as Pawn)!.SetAnimParameter( "pushbattery", true );
	}

	private async void DisableRender()
	{
		await Task.DelaySeconds( 1f );
		EnableDrawing = false;
	}
}

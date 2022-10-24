using GvarJam.Inventory;

namespace GvarJam.Interactions;

/// <summary>
/// Represents an obtainable item.
/// </summary>
[HammerEntity]
[EditorModel( "models/items/battery/battery.vmdl" )]
public sealed partial class Item : DelayedUseItem
{
	/// <inheritdoc/>
	public override float TimeToUse => 0.6f;
	/// <inheritdoc/>
	protected override bool DeleteOnUse => true;

	/// <summary>
	/// The type of item that is being contained.
	/// </summary>
	[Property]
	public ItemType ItemType
	{
		get => itemType;
		set
		{
			itemType = value;
			SetModel( value.GetModel() );
		}
	}
	/// <summary>
	/// See <see cref="ItemType"/>.
	/// </summary>
	[Net]
	private ItemType itemType { get; set; } = ItemType.KeycardLvl1;

	/// <summary>
	/// The amount of the item that is contained.
	/// </summary>
	[Net, Property]
	public int Amount { get; set; } = 1;

	/// <summary>
	/// Whether or not the battery was picked up.
	/// </summary>
	private bool pickedUp;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		Name = ItemType.ToString();

		SetModel( ItemType.GetModel() );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
	}

	/// <inheritdoc/>
	protected override void CreateActions()
	{
		base.CreateActions();

		Actions.Add( (0.6f, PickupItem) );
	}

	/// <inheritdoc/>
	protected override void OnUsed( Entity user )
	{
		base.OnUsed( user );

		(user as Pawn)!.GiveItem( ItemType, Amount );
	}

	/// <summary>
	/// The only animation, picks up the item.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	/// <param name="firstTime">Whether or not this has been invoked for the first time.</param>
	/// <param name="timeInAnim">The time in seconds that this animation has been going for,</param>
	private bool PickupItem( Entity user, bool firstTime, float timeInAnim )
	{
		if ( IsServer && timeInAnim >= 0.55 && !pickedUp )
			PickupItemEntity( user );

		(user as Pawn)!.SetAnimParameter( "grabitem", true );
		return false;
	}

	/// <summary>
	/// Picks up the item.
	/// </summary>
	/// <param name="user">The entity that is doing the interaction.</param>
	private void PickupItemEntity( Entity user )
	{
		pickedUp = true;

		Tags.Add( "camignore" );
		SetParent( user, true );
		Scale = 0.6f;
	}
}

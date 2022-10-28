namespace GvarJam.Interactions;

/// <summary>
/// Represents an obtainable item.
/// </summary>
[Library( "ent_item" )]
[HammerEntity]
[EditorModel( "models/keycards/keycard.vmdl" )] 
public sealed partial class InventoryItem : DelayedUseItem
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
			Name = value.ToString();

			if ( !IsServer || !spawned )
				return;

			SetModel( value.GetModel() );
			SetMaterialGroup( value.GetMaterialGroup() );
			SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
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

	/// <summary>
	/// Server variable for whether or not the spawn method has finished.
	/// </summary>
	private bool spawned;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		ItemType = itemType;
		SetModel( ItemType.GetModel() );
		SetMaterialGroup( ItemType.GetMaterialGroup() );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic );
		spawned = true;
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
		Sound.FromEntity( "item_pickup", user );
		pickedUp = true;

		Tags.Add( "camignore" );
		SetParent( user, true );
		Scale = 0.6f;
	}
}

namespace GvarJam.Interactions;

public sealed class InventoryItemComponent : DelayedUseComponent
{
	public override string LegacyTypeName => "InventoryItem";
	public override float TimeToUse => 0.6f;
	protected override bool DeleteOnUse => true;

	[Property, Sync] public ItemType ItemType { get; set; } = ItemType.KeycardLvl1;
	[Property, Sync] public int Amount { get; set; } = 1;

	private bool _pickedUp;

	protected override void OnStart()
	{
		base.OnStart();
		ApplyVisuals();
	}

	public void ApplyVisuals()
	{
		DisplayName = ItemType.GetItemName();
		var model = Model.Load( ItemType.GetModel() );
		var mr = GameObject.GetOrAddComponent<ModelRenderer>();
		mr.Model = model;
		mr.MaterialGroup = ItemType.GetMaterialGroup();

		var col = GameObject.GetOrAddComponent<ModelCollider>();
		col.Model = model;

		var rb = GameObject.GetOrAddComponent<Rigidbody>();
		rb.MotionEnabled = false;
	}

	protected override void CreateActions()
	{
		Actions.Add( (0.6f, PickupItem) );
	}

	protected override void OnUsed( ColdOpenPlayer user )
	{
		base.OnUsed( user );
		user.Inventory.GiveItem( ItemType, Amount );
	}

	private bool PickupItem( ColdOpenPlayer user, bool firstTime, float timeInAnim )
	{
		if ( timeInAnim >= 0.50f && !_pickedUp )
		{
			AudioDirector.PlayAt( "item_pickup", user.WorldPosition );
			_pickedUp = true;
			GameObject.Tags.Add( "camignore" );
			WorldScale = 0.6f;
		}

		user.BodyRenderer?.Set( "grabitem", true );
		return false;
	}
}

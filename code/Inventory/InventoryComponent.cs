namespace GvarJam.Inventory;

public sealed class InventoryComponent : Component
{
	[Sync] public NetDictionary<int, int> Items { get; set; } = new();

	protected override void OnStart()
	{
		if ( IsProxy ) return;
		foreach ( var itemType in Enum.GetValues<ItemType>() )
		{
			Items.TryAdd( (int)itemType, 0 );
		}
	}

	public void GiveItem( ItemType itemType, int amount = 1 )
	{
		var key = (int)itemType;
		Items[key] = Items.TryGetValue( key, out var cur ) ? cur + amount : amount;
	}

	public bool HasItem( ItemType item, int amount = 1 )
	{
		return Items.TryGetValue( (int)item, out var cur ) && cur >= amount;
	}

	public bool TryTakeItem( ItemType item )
	{
		var key = (int)item;
		if ( !Items.TryGetValue( key, out var cur ) || cur <= 0 )
			return false;

		Items[key] = cur - 1;
		return true;
	}
}

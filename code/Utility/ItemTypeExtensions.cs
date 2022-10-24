namespace GvarJam.Utility;
	
/// <summary>
/// Extension class for <see cref="ItemType"/>.
/// </summary>
public static class ItemTypeExtensions
{
	/// <summary>
	/// Gets the model associated with an item.
	/// </summary>
	/// <param name="itemType">The item to get the model of.</param>
	/// <returns>The model of the item.</returns>
	public static string GetModel( this ItemType itemType )
	{
		return itemType switch
		{
			ItemType.KeycardLvl1 => "models/items/battery/battery.vmdl",
			ItemType.KeycardLvl2 => "models/items/battery/battery.vmdl",
			ItemType.KeycardLvl3 => "models/items/battery/battery.vmdl",
			_ => throw new ArgumentException( null, nameof(itemType) )
		};
	}
}

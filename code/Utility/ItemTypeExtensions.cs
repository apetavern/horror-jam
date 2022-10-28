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
			ItemType.JanitorKey => "models/keycards/keycard.vmdl",
			ItemType.KeycardLvl1 => "models/keycards/keycard.vmdl",
			ItemType.KeycardLvl2 => "models/keycards/keycard.vmdl",
			ItemType.KeycardLvl3 => "models/keycards/keycard.vmdl",
			ItemType.GeneratorFuse => "models/generator/fuse_01a.vmdl",
			_ => throw new ArgumentException( null, nameof(itemType) )
		};
	}

	public static int GetMaterialGroup( this ItemType itemType )
	{
		return itemType switch
		{
			ItemType.JanitorKey => 0,
			ItemType.KeycardLvl1 => 1,
			ItemType.KeycardLvl2 => 2,
			ItemType.KeycardLvl3 => 3,
			ItemType.GeneratorFuse => 0,
			_ => throw new ArgumentException( null, nameof( itemType ) )
		};
	}
}

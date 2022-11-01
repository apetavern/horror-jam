namespace GvarJam.Utility;
	
/// <summary>
/// Extension class for <see cref="ItemType"/>.
/// </summary>
public static class ItemTypeExtensions
{
	/// <summary>
	/// Returns the formatted name of the provided <see cref="ItemType"/>.
	/// </summary>
	/// <param name="itemType">The type of item to get the item name on.</param>
	/// <returns>The formatted name of the provided <see cref="ItemType"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the <see cref="ItemType"/> provided is invalid.</exception>
	public static string GetItemName( this ItemType itemType )
	{
		return itemType switch
		{
			ItemType.JanitorKey => "Keycard - Janitor",
			ItemType.KeycardLvl1 => "Keycard - Level 1",
			ItemType.KeycardLvl2 => "Keycard - Level 2",
			ItemType.KeycardLvl3 => "Keycard - Level 3",
			ItemType.GeneratorFuse => "Generator Fuse",
			_ => throw new ArgumentException( null, nameof( itemType ) )
		};
	}

	/// <summary>
	/// Returns the material group corresponding to the provided <see cref="ItemType"/>.
	/// </summary>
	/// <param name="itemType">The type of item to get the material group on.</param>
	/// <returns>The material group corresponding to the provided <see cref="ItemType"/>.</returns>
	/// <exception cref="ArgumentException">Thrown when the <see cref="ItemType"/> provided is invalid.</exception>
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

	/// <summary>
	/// Gets the model associated with an item.
	/// </summary>
	/// <param name="itemType">The item to get the model of.</param>
	/// <returns>The model of the item.</returns>
	/// <exception cref="ArgumentException">Thrown when the <see cref="ItemType"/> provided is invalid.</exception>
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
}

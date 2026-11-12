namespace GvarJam.Utility;

public static class ItemTypeExtensions
{
	public static string GetItemName( this ItemType itemType ) => itemType switch
	{
		ItemType.JanitorKey => "Keycard - Janitor",
		ItemType.KeycardLvl1 => "Keycard - Level 1",
		ItemType.KeycardLvl2 => "Keycard - Level 2",
		ItemType.KeycardLvl3 => "Keycard - Level 3",
		ItemType.GeneratorFuse => "Generator Fuse",
		_ => throw new ArgumentException( null, nameof( itemType ) )
	};

	public static string GetMaterialGroup( this ItemType itemType ) => itemType switch
	{
		ItemType.JanitorKey => "Janitor",
		ItemType.KeycardLvl1 => "Level1",
		ItemType.KeycardLvl2 => "Level2",
		ItemType.KeycardLvl3 => "Level3",
		ItemType.GeneratorFuse => "default",
		_ => "default"
	};

	public static string GetModel( this ItemType itemType ) => itemType switch
	{
		ItemType.JanitorKey => "models/keycards/keycard.vmdl",
		ItemType.KeycardLvl1 => "models/keycards/keycard.vmdl",
		ItemType.KeycardLvl2 => "models/keycards/keycard.vmdl",
		ItemType.KeycardLvl3 => "models/keycards/keycard.vmdl",
		ItemType.GeneratorFuse => "models/generator/fuse_01a.vmdl",
		_ => throw new ArgumentException( null, nameof( itemType ) )
	};
}

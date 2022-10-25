namespace GvarJam.Player;

public partial class Pawn
{
	/// <summary>
	/// The items that the pawn currently has.
	/// </summary>
	[Net]
	IDictionary<ItemType, int> Items { get; set; } = null!;

	/// <summary>
	/// Gives the pawn an item.
	/// </summary>
	/// <param name="itemType">The item to give.</param>
	/// <param name="amount">The amount of the item to give.</param>
	public void GiveItem( ItemType itemType, int amount = 1 )
	{
		Items[itemType]++;
	}

	/// <summary>
	/// Returns whether or not the pawn has an item.
	/// </summary>
	/// <param name="item">The item to check if the pawn has.</param>
	/// <param name="amount">The amount of the item the pawn has.</param>
	/// <returns>Whether or not the pawn has the item.</returns>
	public bool HasItem( ItemType item, int amount = 1 )
	{
		return Items[item] >= amount;
	}

	/// <summary>
	/// Attemptes to take an item the pawn has.
	/// </summary>
	/// <param name="item">The item to take.</param>
	/// <returns>Whether or not the item was taken.</returns>
	public bool TryTakeItem( ItemType item )
	{
		if ( Items[item] == 0 )
			return false;

		Items[item]--;
		return true;
	}
}

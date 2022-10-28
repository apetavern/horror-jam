﻿namespace GvarJam.Inventory;

/// <summary>
/// Represents a type of item.
/// </summary>
public enum ItemType
{
	/// <summary>
	/// A key used to unlock locked lockers.
	/// </summary>
	JanitorKey,
	/// <summary>
	/// A basic level 1 keycard.
	/// </summary>
	KeycardLvl1,
	/// <summary>
	/// An intermediate level 2 keycard.
	/// </summary>
	KeycardLvl2,
	/// <summary>
	/// An advanced level 3 keycard.
	/// </summary>
	KeycardLvl3,
	/// <summary>
	/// A fuse to repair the generator.
	/// </summary>
	GeneratorFuse,
}

using Sandbox;

namespace GvarJam.Interactions;

/// <summary>
/// A contract to define something that has an interaction time.
/// </summary>
public interface IInteractable : IUse
{
	/// <summary>
	/// Resets the current interaction.
	/// </summary>
	void Reset();
}

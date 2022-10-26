namespace GvarJam.Interactions;

/// <summary>
/// A contract to define something that is interactable.
/// </summary>
public interface IInteractable : IUse
{
	/// <summary>
	/// Resets the current interaction (if applicable).
	/// </summary>
	void Reset();
}

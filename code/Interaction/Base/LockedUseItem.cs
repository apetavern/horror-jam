namespace GvarJam.Interactions;

public partial class LockedUseItem : InteractableEntity
{
	public virtual void StopUsing()
	{
		User = null;
	}

	public virtual void Simulate() { }
}


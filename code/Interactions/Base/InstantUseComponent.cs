namespace GvarJam.Interactions;

public abstract class InstantUseComponent : InteractableComponent
{
	protected virtual bool DeleteOnUse => true;

	public override bool OnUse( ColdOpenPlayer user )
	{
		OnUsed( user );

		if ( DeleteOnUse && GameObject.IsValid() )
			GameObject.Destroy();

		return false;
	}

	public override void ResetInteraction()
	{
	}
}

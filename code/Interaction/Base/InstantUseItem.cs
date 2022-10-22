namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that has an instant use.
/// </summary>
[Category( "Interactables" )]
public class InstantUseItem : InteractableEntity, IInteractable
{
	/// <summary>
	/// Whether or not to delete the entity after it has been used.
	/// </summary>
	protected virtual bool DeleteOnUse => true;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public override bool OnUse( Entity user )
	{
		OnUsed( user );

		if ( IsServer && DeleteOnUse )
			Delete();

		return false;
	}

	public override void Reset()
	{
		throw new NotSupportedException();
	}
}

namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that has an instant use.
/// </summary>
public class InstantUseItem : InteractableEntity
{
	/// <summary>
	/// Whether or not to delete the entity after it has been used.
	/// </summary>
	protected virtual bool DeleteOnUse => true;

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	public override bool OnUse( Entity user )
	{
		OnUsed( user );

		if ( IsServer && DeleteOnUse )
			Delete();

		return false;
	}

	/// <inheritdoc/>
	public override void Reset()
	{
		throw new NotSupportedException();
	}
}

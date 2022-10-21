namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that has an instant use.
/// </summary>
[Category( "Interactables" )]
public class InstantUseItem : GlowItem, IInteractable
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

	/// <summary>
	/// Invoked when the item has been used.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUsed( Entity user )
	{
		DebugOverlay.Text( "Used", Position, 1 );
	}

	/// <inheritdoc/>
	public virtual bool IsUsable( Entity user )
	{
		return user.Position.Distance( Position ) < 100;
	}

	/// <inheritdoc/>
	public bool OnUse( Entity user )
	{
		OnUsed( user );
		if ( IsServer && DeleteOnUse )
			Delete();

		return false;
	}

	/// <inheritdoc/>
	public void Reset()
	{
		throw new NotSupportedException();
	}
}

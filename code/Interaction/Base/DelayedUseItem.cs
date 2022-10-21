namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Category( "Interactables" )]
public partial class DelayedUseItem : GlowItem, IInteractable
{
	/// <summary>
	/// The amount of time it takes to use the item.
	/// </summary>
	public virtual float TimeToUse => 1;

	/// <summary>
	/// Whether or not to delete the entity once the item has been used.
	/// </summary>
	protected virtual bool DeleteOnUse => true;

	/// <summary>
	/// Whether or not to reset the <see cref="CurrentUseTime"/> once the item has been used.
	/// <remarks>This will be irrelevant if <see cref="DeleteOnUse"/> is true.</remarks>
	/// </summary>
	protected virtual bool ResetOnUse => true;

	/// <summary>
	/// The entity that is currently using the item.
	/// </summary>
	[Net]
	public Entity? User { get; protected set; }

	/// <summary>
	/// The current time that the item has been used for.
	/// </summary>
	[Net, Predicted]
	protected float CurrentUseTime { get; set; }

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <summary>
	/// Invoked on each tick that the item is being used.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUseTick( Entity user )
	{
		DebugOverlay.Text( $"Use: {MathX.Floor(CurrentUseTime / TimeToUse * 100)}%\nUser: {user}", Position );
	}

	/// <summary>
	/// Invoked when using the item has finished.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUsed( Entity user )
	{
		DebugOverlay.Text( "Used", Position, 1 );
	}

	/// <inheritdoc/>
	public virtual bool IsUsable( Entity user )
	{
		if ( User is not null && User != user )
			return false;

		return user.Position.Distance( Position ) < 100;
	}

	/// <inheritdoc/>
	public bool OnUse( Entity user )
	{
		if ( IsServer )
			User = user;

		CurrentUseTime += Time.Delta;
		OnUseTick( user );

		var used = CurrentUseTime >= TimeToUse;
		if ( used )
		{
			OnUsed( user );
			if ( IsServer && DeleteOnUse )
				Delete();
			else if ( ResetOnUse )
				Reset();
		}

		return !used;
	}

	/// <inheritdoc/>
	public virtual void Reset()
	{
		CurrentUseTime = 0;
		if ( IsServer )
			User = null;
	}
}

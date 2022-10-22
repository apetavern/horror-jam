using System.Collections.Generic;

namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
[Category( "Interactables" )]
public partial class DelayedUseItem : InteractableEntity, IInteractable
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
	/// The list of animations to play on the user when interacting with this item.
	/// </summary>
	protected virtual List<(float, Action<Entity>)> Actions => new();

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
		for ( var i = 0; i < Actions.Count; i++ )
		{
			var totalTime = 0f;
			for ( var j = i; j > 0; j-- )
				totalTime += Actions[j].Item1;

			var nextTime = 0f;
			for ( var j = i + 1; j > 0; j-- )
			{
				if ( j >= Actions.Count )
				{
					nextTime = TimeToUse;
					break;
				}

				nextTime += Actions[j].Item1;
			}

			if ( CurrentUseTime >= totalTime && CurrentUseTime < nextTime )
				Actions[i].Item2( user );
		}
	}

	public override bool IsUsable( Entity user )
	{
		if ( User is not null && User != user )
			return false;

		return user.Position.Distance( Position ) < 100;
	}

	public override bool OnUse( Entity user )
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

	public override void Reset()
	{
		CurrentUseTime = 0;

		if ( IsServer )
			User = null;
	}

	/// <summary>
	/// Returns current interaction progress from 0 .. 1
	/// </summary>
	public float GetInteractionProgress()
	{
		return CurrentUseTime / TimeToUse;
	}
}

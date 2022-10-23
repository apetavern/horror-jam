namespace GvarJam.Interactions;

/// <summary>
/// Represents an item that takes time to use.
/// </summary>
public partial class DelayedUseItem : InteractableEntity
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
	/// Whether or not to reset the interaction once the item has been used.
	/// <remarks>This will be irrelevant if <see cref="DeleteOnUse"/> is true.</remarks>
	/// </summary>
	protected virtual bool ResetOnUse => true;

	/// <summary>
	/// The list of animations to play on the user when interacting with this item.
	/// </summary>
	protected List<(float, Func<Entity, bool, float, bool>)> Actions = new();

	/// <summary>
	/// The current time that the item has been used for.
	/// </summary>
	[Net, Predicted]
	protected float CurrentUseTime { get; set; }

	/// <summary>
	/// The index of the current action in the interaction.
	/// </summary>
	[Net, Predicted]
	private int CurrentActionIndex { get; set; }

	/// <summary>
	/// A hash set of all the action indices that have been called for the first time.
	/// </summary>
	private readonly HashSet<int> firstTimeActions = new();

	/// <inheritdoc/>
	public override void Spawn()
	{
		base.Spawn();

		CreateActions();

		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	/// <inheritdoc/>
	public override void ClientSpawn()
	{
		base.ClientSpawn();

		CreateActions();
	}

	/// <summary>
	/// Returns current interaction progress from 0 .. 1
	/// </summary>
	public float GetInteractionProgress()
	{
		return CurrentUseTime / TimeToUse;
	}

	/// <summary>
	/// Invoked to create any required actions for the interaction.
	/// </summary>
	protected virtual void CreateActions()
	{
	}

	/// <summary>
	/// Invoked on each tick that the item is being used.
	/// </summary>
	/// <param name="user">The entity that is using the item.</param>
	protected virtual void OnUseTick( Entity user )
	{
		if ( CurrentActionIndex >= Actions.Count )
			return;

		var previousTotalTime = 0f;
		for ( var i = 0; i < CurrentActionIndex; i++ )
			previousTotalTime += Actions[i].Item1;

		if ( previousTotalTime + Actions[CurrentActionIndex].Item1 < CurrentUseTime )
			CurrentActionIndex++;

		if ( CurrentActionIndex >= Actions.Count )
			return;

		var result = Actions[CurrentActionIndex].Item2( user, firstTimeActions.Contains( CurrentActionIndex ), CurrentUseTime - previousTotalTime );
		firstTimeActions.Add( CurrentActionIndex );
		if ( !result )
			return;

		CurrentUseTime = previousTotalTime + Actions[CurrentActionIndex].Item1;
		CurrentActionIndex++;
	}

	/// <inheritdoc/>
	public override bool IsUsable( Entity user )
	{
		if ( User is not null && User != user )
			return false;

		return user.Position.Distance( Position ) < 100;
	}

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public override void Reset()
	{
		CurrentUseTime = 0;
		CurrentActionIndex = 0;
		firstTimeActions.Clear();
		if ( IsServer )
			User = null;
	}
}

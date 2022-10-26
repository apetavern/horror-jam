namespace GvarJam.Objectives;

/// <summary>
/// Represents an objective in the game to complete.
/// </summary>
public sealed class Objective
{
	/// <summary>
	/// The underlying resource of the objective.
	/// </summary>
	public ObjectiveResource Resource { get; set; } = null!;

	/// <summary>
	/// The time since the objective was started.
	/// </summary>
	private TimeSince TimeSinceObjectiveStart;

	/// <summary>
	/// The time since the objective was finished.
	/// </summary>
	private TimeSince TimeSinceObjectiveEnd;

	/// <inheritdoc/>
	public override bool Equals( object? obj )
	{
		if ( obj is not Objective objective )
			return false;

		return Resource == objective.Resource;
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return Resource.GetHashCode();
	}

	/// <summary>
	/// Run through all the inactive objectives and check if all the start
	/// conditions are currently being met
	/// </summary>
	public bool CheckStart( Pawn pawn )
	{
		bool conditionsMet = true;

		foreach ( var condition in Resource.ObjectiveStartConditions )
		{
			if ( !condition.IsMet( pawn ) )
			{
				conditionsMet = false;
				break;
			}
		}

		return conditionsMet;
	}

	/// <summary>
	/// Run through all the active objectives and check if all the end
	/// conditions are currently being met
	/// </summary>
	public bool CheckEnd( Pawn pawn )
	{
		bool conditionsMet = true;

		foreach ( var condition in Resource.ObjectiveEndConditions )
		{
			if ( !condition.IsMet( pawn ) )
			{
				conditionsMet = false;
				break;
			}
		}

		return conditionsMet;
	}

	/// <summary>
	/// Invokes the event methods for pawns to start the objective.
	/// </summary>
	/// <param name="pawn">The pawn to invoke the event methods for.</param>
	public void InvokeStartEvents( Pawn pawn )
	{
		Resource.ObjectiveStartEvents?.ForEach( x => x.Invoke( pawn ) );

		TimeSinceObjectiveStart = 0;
	}

	/// <summary>
	/// Invokes the event methods for pawns to finish the objective.
	/// </summary>
	/// <param name="pawn">The pawn to invoke the event methods for.</param>
	public void InvokeEndEvents( Pawn pawn )
	{
		Resource.ObjectiveEndEvents?.ForEach( x => x.Invoke( pawn ) );

		TimeSinceObjectiveEnd = 0;
	}
}

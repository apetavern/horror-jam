namespace GvarJam.Objectives;

public class Objective
{
	public ObjectiveResource Resource { get; set; }

	private TimeSince TimeSinceObjectiveStart;
	private TimeSince TimeSinceObjectiveEnd;

	public override bool Equals( object? obj )
	{
		if ( obj is not Objective objective )
			return false;

		return Resource == objective.Resource;
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

	public void InvokeStartEvents( Pawn pawn )
	{
		Resource.ObjectiveStartEvents?.ForEach( x => x.Invoke( pawn ) );

		TimeSinceObjectiveStart = 0;
	}

	public void InvokeEndEvents( Pawn pawn )
	{
		Resource.ObjectiveEndEvents?.ForEach( x => x.Invoke( pawn ) );

		TimeSinceObjectiveEnd = 0;
	}
}

namespace GvarJam.Objectives;

public sealed class Objective
{
	public ObjectiveResource Resource { get; set; } = null!;
	private TimeSince TimeSinceObjectiveStart;
	private TimeSince TimeSinceObjectiveEnd;

	public override bool Equals( object obj )
	{
		return obj is Objective other && Resource == other.Resource;
	}

	public override int GetHashCode() => Resource.GetHashCode();

	public bool CheckStart( ColdOpenPlayer player )
	{
		foreach ( var condition in Resource.ObjectiveStartConditions )
		{
			if ( !condition.IsMet( player ) )
				return false;
		}
		return Resource.ObjectiveStartConditions.Count > 0;
	}

	public bool CheckEnd( ColdOpenPlayer player )
	{
		foreach ( var condition in Resource.ObjectiveEndConditions )
		{
			if ( !condition.IsMet( player, TimeSinceObjectiveStart ) )
				return false;
		}
		return true;
	}

	public void InvokeStartEvents( ColdOpenPlayer player )
	{
		Resource.ObjectiveStartEvents?.ForEach( x => x.Invoke( player ) );
		TimeSinceObjectiveStart = 0;
	}

	public void InvokeEndEvents( ColdOpenPlayer player )
	{
		Resource.ObjectiveEndEvents?.ForEach( x => x.Invoke( player ) );
		TimeSinceObjectiveEnd = 0;
	}
}

namespace GvarJam.Objectives;

//
// Objectives are server-side objects that remain the same
// for all players on the server. Ideally they'd just be
// part of Game, but I thought it'd be nicer to separate the
// whole system out into it's own class, so that's done here.
//
// Clients have no knowledge of this system; instead, whenever
// an objective is added or completed, an RPC gets passed along
// to the client informing them of this, and the UI gets changed
// accordingly.
//

public class ObjectiveSystem
{
	public static ObjectiveSystem Current { get; set; }

	public IEnumerable<Pawn> Pawns => Client.All.Select( x => x.Pawn ).OfType<Pawn>();

	/// <summary>
	/// Which objectives haven't been completed yet?
	/// </summary>
	private List<ObjectiveResource> PendingObjectives { get; } = ResourceLibrary.GetAll<ObjectiveResource>().ToList();

	/// <summary>
	/// Which objectives are the player/s doing?
	/// </summary>
	private List<ObjectiveResource> ActiveObjectives { get; } = new();

	public ObjectiveSystem()
	{
		Current = this;
		Event.Register( this );
	}

	[Event.Tick.Server]
	public void OnServerTick()
	{
		int line = 5;
		void PrintObjective( ObjectiveResource objective )
		{
			DebugOverlay.ScreenText( $"Objective: {objective.Name}", line++ );
			DebugOverlay.ScreenText( $"{objective.Description}", line++ );
		}

		DebugOverlay.ScreenText( $"Objectives:", line++ );
		foreach ( var objective in ActiveObjectives )
		{
			PrintObjective( objective );
		}

		DebugOverlay.ScreenText( $"Pending objective count: {PendingObjectives.Count}", line++ );
		DebugOverlay.ScreenText( $"Active objective count: {ActiveObjectives.Count}", line++ );

		foreach ( var pawn in Pawns )
		{
			CheckStartObjectives( pawn );
			CheckEndObjectives( pawn );
		}
	}

	/// <summary>
	/// Run through all the inactive objectives and check if all the start
	/// conditions are currently being met
	/// </summary>
	public void CheckStartObjectives( Pawn pawn )
	{
		foreach ( var objective in PendingObjectives.ToList() )
		{
			bool conditionsMet = true;

			foreach ( var condition in objective.ObjectiveStartConditions )
			{
				if ( !condition.IsMet( pawn ) )
				{
					conditionsMet = false;
					break;
				}
			}

			if ( conditionsMet )
			{
				Log.Trace( $"Start objective {objective}" );

				ActiveObjectives.Add( objective );
				PendingObjectives.Remove( objective );
			}
		}
	}

	/// <summary>
	/// Run through all the active objectives and check if all the end
	/// conditions are currently being met
	/// </summary>
	public void CheckEndObjectives( Pawn pawn )
	{
		foreach ( var objective in ActiveObjectives.ToList() )
		{
			bool conditionsMet = true;

			foreach ( var condition in objective.ObjectiveEndConditions )
			{
				if ( !condition.IsMet( pawn ) )
				{
					conditionsMet = false;
					break;
				}
			}

			if ( conditionsMet )
			{
				Log.Trace( $"End objective {objective}" );

				ActiveObjectives.Remove( objective );
			}
		}
	}
}

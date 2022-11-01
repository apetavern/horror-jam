using GvarJam.UI.Elements;

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
/// <summary>
/// The system to control objectives and handle their lifetime/completion.
/// </summary>
public sealed class ObjectiveSystem
{
	/// <summary>
	/// The only instance of this system in existence.
	/// </summary>
	public static ObjectiveSystem Current { get; set; }

	/// <summary>
	/// Which objectives haven't been completed yet?
	/// </summary>
	/// 
	public List<Objective> PendingObjectives { get; } = ResourceLibrary.GetAll<ObjectiveResource>().Select( x => new Objective() { Resource = x } ).ToList();

	/// <summary>
	/// Which objectives are the player/s doing?
	/// </summary>
	public List<Objective> ActiveObjectives { get; } = new();

	/// <summary>
	/// Initializes a default instance of <see cref="ObjectiveSystem"/>.
	/// </summary>
	public ObjectiveSystem()
	{
		Current = this;
		Event.Register( this );

		PendingObjectives.ForEach( x =>
		{
			Log.Trace( x.Resource.Name );
		} );
	}

	/// <summary>
	/// Run through all the inactive objectives and check if all the start
	/// conditions are currently being met
	/// </summary>
	public void CheckStartObjectives( Pawn pawn )
	{
		foreach ( var objective in PendingObjectives.ToList() )
		{
			if ( objective.CheckStart( pawn ) )
			{
				Log.Trace( $"Start objective {objective}" );

				objective.InvokeStartEvents( pawn );

				ActiveObjectives.Add( objective );
				PendingObjectives.Remove( objective );

				ObjectivePanel.RpcAddObjective( To.Everyone, objective.Resource );
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
			if ( !objective.CheckEnd( pawn ) )
				continue;

			objective.InvokeEndEvents( pawn );
			ActiveObjectives.Remove( objective );
			ObjectivePanel.RpcRemoveObjective( To.Everyone, objective.Resource );
		}
	}

	/// <summary>
	/// Debug method to print objective state.
	/// </summary>
	/// <param name="objective">The objective to output debug info from.</param>
	/// <param name="line">The line number to start at with screen text overlay.</param>
	private void PrintObjective( Objective objective, ref int line )
	{
		DebugOverlay.ScreenText( $"Objective: {objective.Resource.ObjectiveName}", line++ );
		DebugOverlay.ScreenText( $"{objective.Resource.Description}", line++ );
	}

	/// <summary>
	/// Ticks the objective system.
	/// </summary>
	[Event.Tick.Server]
	private void OnServerTick()
	{
		if ( HorrorGame.Debug )
		{
			var line = 5;
			DebugOverlay.ScreenText( $"Objectives:", line++ );
			foreach ( var objective in ActiveObjectives )
				PrintObjective( objective, ref line );

			DebugOverlay.ScreenText( $"Pending objective count: {PendingObjectives.Count}", line++ );
			DebugOverlay.ScreenText( $"Active objective count: {ActiveObjectives.Count}", line++ );
		}

		foreach ( var pawn in Client.All.Select( x => x.Pawn ).OfType<Pawn>() )
		{
			CheckStartObjectives( pawn );
			CheckEndObjectives( pawn );
		}
	}
}

namespace GvarJam.Monster;

partial class MonsterEntity
{
	private States state = States.Hiding;
	private States State
	{
		get => state;
		set
		{
			OnStateChange( state, value );
			state = value;
		}
	}

	private Vector3 TargetPosition;
	private TimeSince TimeInState;

	private void TickState()
	{
		switch ( State )
		{
			case States.Hiding:
				SimulateHidingState();
				break;
			case States.Stalking:
				SimulateStalkingState();
				break;
			case States.Hunting:
				SimulateHuntingState();
				break;
		}
	}

	private void OnStateChange( States oldState, States newState )
	{
		Log.Trace( $"Monster state changed from {oldState} to {newState}" );

		TimeInState = 0;

		if ( oldState == States.Hiding )
		{
			// Teleport back into the map
			Game.Current.MoveToSpawnpoint( this );
		}

		if ( newState == States.Stalking )
		{
			// Find a target position that isn't too close to the player
			var targetPlayer = Entity.All.OfType<Pawn>().First();
			TargetPosition = NavMesh.GetPointWithinRadius( targetPlayer.Position, 512, 2048 ) ?? 0;

			SetPath( TargetPosition );
		}

		if ( newState == States.Hunting )
		{
			//
		}
	}

	/// <summary>
	/// In this state, the monster is effectively moved out of the map.
	/// It is 'hiding' from the player, invoking an air of uncertainty
	/// and tension.
	/// </summary>
	private void SimulateHidingState()
	{
		Position = new Vector3( 16384, 16384, 16384 );
		Velocity = 0;

		// TODO: Make exit conditions more complex
		if ( TimeInState > 30 )
		{
			State = States.Stalking;
		}
	}

	/// <summary>
	/// In this state, the monster is slowly making its way towards the
	/// player. If the monster gets close to the player, it will stop
	/// and wait for the player to get close before proceeding. The
	/// monster will attempt to be 'visible' to the player in some way.
	/// </summary>
	private void SimulateStalkingState()
	{
		PathFinding.Speed = 100f;

		// TODO: Make exit conditions more complex
		if ( TimeInState > 60 )
		{
			State = States.Hunting;
		}
	}

	/// <summary>
	/// In this state, the monster is actively chasing down the player.
	/// This state has a maximum duration of 30 seconds, before the
	/// monster runs off and goes back into a hiding state.
	/// </summary>
	private void SimulateHuntingState()
	{
		// TODO:
		// - Stealth logic:
		//   - If the player is crouching under a table, we shouldn't know where they are.
		//   - If the player runs around a corner, we only know about their 'last seen'
		//     position, not their actual position.
		//   - We should be attracted to loud noises.
		// - If the player can see us, we shouldn't move to a hiding state

		var targetPlayer = Entity.All.OfType<Pawn>().First();
		SetPath( targetPlayer.Position );

		PathFinding.Speed = 250f;

		// TODO: Make exit conditions more complex
		if ( TimeInState > 30 )
		{
			State = States.Hiding;
		}
	}
}

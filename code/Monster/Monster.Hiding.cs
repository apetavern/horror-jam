namespace GvarJam.Monster;

partial class MonsterEntity
{
	private States state = States.Dormant;
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
	private TimeSince TimeSinceSawPlayer;

	private void TickState()
	{
		switch ( State )
		{
			case States.Hiding:
				SimulateHidingState();
				break;
			case States.Stalking:
				TickSeen();
				SimulateStalkingState();
				break;
			case States.Hunting:
				TickSeen();
				SimulateHuntingState();
				break;
		}
	}

	/// <summary>
	/// Can we see the player? If so, chase after them
	/// </summary>
	private void TickSeen()
	{
		const float FieldOfView = 90f;
		const int TraceCount = 16;
		const float Radius = 4f;

		const float DefaultDistance = 256;
		const float DistanceWithFlashlight = 1024;

		foreach ( var entity in Entity.FindInSphere( Position, 128f ) )
		{
			if ( entity is Pawn pawn )
			{
				// Player is REALLY close, check for LOS and chase after them
				var startPos = EyePosition;
				var endPos = pawn.EyePosition;

				var tr = Trace.Ray( startPos, endPos ).Radius( Radius ).Ignore( this ).Run();

				if ( tr.Hit && tr.Entity == pawn )
				{
					TargetPosition = pawn.Position;
					State = States.Hunting;
					TimeSinceSawPlayer = 0;

					return;
				}
			}
		}

		//
		// Can we actually see a player right now
		//
		for ( int i = 0; i < TraceCount; ++i )
		{
			float f = (float)i / (TraceCount - 1);
			float ang = (f * FieldOfView) - (FieldOfView / 2f);
			var direction = (Rotation * Rotation.FromYaw( ang )).Forward;

			var startPos = EyePosition + Vector3.Down * 16f;
			var endPos = startPos + direction * DistanceWithFlashlight;

			var tr = Trace.Ray( startPos, endPos ).Radius( Radius ).Ignore( this ).Run();

			if ( tr.Hit && tr.Entity is Pawn pawn )
			{
				//
				// This is a bit messy/hacky, but if the player has their flashlight on,
				// then we can see them at a further distance.
				//

				if ( tr.Distance > DefaultDistance && !pawn.LampEnabled )
					continue;

				TargetPosition = pawn.Position;
				State = States.Hunting;
				TimeSinceSawPlayer = 0;
			}
		}
	}

	private void OnStateChange( States oldState, States newState )
	{
		Log.Trace( $"Monster state changed from {oldState} to {newState}" );

		TimeInState = 0;
		AverageSpeed = 50f;

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
			// We know EXACTLY where the player is right now. Bee-line it to them
			TargetPosition = Entity.All.OfType<Pawn>().First().Position;

			SetPath( TargetPosition );
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

		SetPath( TargetPosition );
		PathFinding.Speed = 200f;

		var startPos = EyePosition;
		var endPos = Entity.All.OfType<Pawn>().First().EyePosition;

		var tr = Trace.Ray( startPos, endPos ).Ignore( this ).Run();

		if ( tr.Hit && tr.Entity is Pawn pawn )
		{
			TargetPosition = pawn.Position;
		}

		if ( PathFinding.IsFinished && TimeSinceSawPlayer > 5f )
		{
			State = States.Stalking;
		}
	}
}
